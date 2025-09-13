using Microsoft.EntityFrameworkCore;
using QueueWorker.Data;
using QueueWorker.Models;
using System.Text.Json;

namespace QueueWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing messages");
            }

            // Esperar 5 segundos antes de verificar nuevos mensajes
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ProcessMessagesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Obtener mensajes no procesados
        var unprocessedMessages = await dbContext.MessageQueue
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        foreach (var message in unprocessedMessages)
        {
            await ProcessSingleMessageAsync(dbContext, message);
        }

        if (unprocessedMessages.Any())
        {
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task ProcessSingleMessageAsync(ApplicationDbContext dbContext, MessageQueue message)
    {
        try
        {
            _logger.LogInformation("Processing message {MessageId} with correlation {CorrelationId}", 
                message.MessageId, message.CorrelationId);

            // Deserializar el mensaje
            var productMessage = JsonSerializer.Deserialize<ProductMessage>(message.MessageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (productMessage == null)
            {
                _logger.LogWarning("Could not deserialize message {MessageId}", message.MessageId);
                return;
            }

            // Buscar si el producto ya existe (idempotencia)
            var existingProduct = await dbContext.Products
                .FirstOrDefaultAsync(p => p.ExternalId == productMessage.ExternalId);

            if (existingProduct != null)
            {
                // Actualizar producto existente
                existingProduct.Name = productMessage.Name;
                existingProduct.Price = productMessage.Price;
                existingProduct.Currency = productMessage.Currency;
                existingProduct.UpdatedAtUtc = productMessage.FetchedAtUtc;
                existingProduct.ProcessedAtUtc = DateTime.UtcNow;

                _logger.LogInformation("Updated existing product {ExternalId} - CorrelationId: {CorrelationId}", 
                    productMessage.ExternalId, message.CorrelationId);
            }
            else
            {
                // Insertar nuevo producto
                var newProduct = new Product
                {
                    ExternalId = productMessage.ExternalId,
                    Name = productMessage.Name,
                    Price = productMessage.Price,
                    Currency = productMessage.Currency,
                    UpdatedAtUtc = productMessage.FetchedAtUtc,
                    ProcessedAtUtc = DateTime.UtcNow
                };

                dbContext.Products.Add(newProduct);
                
                _logger.LogInformation("Inserted new product {ExternalId} - CorrelationId: {CorrelationId}", 
                    productMessage.ExternalId, message.CorrelationId);
            }

            // Marcar mensaje como procesado
            message.IsProcessed = true;
            message.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId} with correlation {CorrelationId}", 
                message.MessageId, message.CorrelationId);
        }
    }
}

// Clase para deserializar mensajes
public class ProductMessage
{
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FetchedAtUtc { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}