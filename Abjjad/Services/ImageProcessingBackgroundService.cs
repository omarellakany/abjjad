using System.Collections.Concurrent;

namespace Abjjad.Services;

public class ImageProcessingBackgroundService(
    ILogger<ImageProcessingBackgroundService> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration)
    : BackgroundService
{
    private readonly int _maxConcurrentProcessing =
        configuration.GetValue("ImageProcessing:MaxConcurrentProcessing", 3);

    private readonly ConcurrentQueue<(Stream Stream, string FileName, string Id)> _processingQueue = new();
    private readonly SemaphoreSlim _processingSemaphore = new(0);

    public void EnqueueImage(Stream stream, string fileName, string id)
    {
        _processingQueue.Enqueue((stream, fileName, id));
        _processingSemaphore.Release();
        logger.LogInformation("Image queued for processing: {FileName} with ID: {Id}", fileName, id);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background service started");

        var processingTasks = new List<Task>();
        var semaphore = new SemaphoreSlim(_maxConcurrentProcessing);

        while (!stoppingToken.IsCancellationRequested)
            try
            {
                await _processingSemaphore.WaitAsync(stoppingToken);
                logger.LogInformation("Processing semaphore released, checking queue");

                while (_processingQueue.TryDequeue(out var item))
                {
                    logger.LogInformation("Dequeued item: {FileName}", item.FileName);

                    await semaphore.WaitAsync(stoppingToken);

                    var task = ProcessImageAsync(item, semaphore);
                    processingTasks.Add(task);

                    processingTasks.RemoveAll(t => t.IsCompleted);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Background service stopping");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in background service");
            }

        logger.LogInformation("Waiting for remaining tasks to complete");
        await Task.WhenAll(processingTasks);
        logger.LogInformation("All tasks completed");
    }

    private async Task ProcessImageAsync((Stream Stream, string FileName, string Id) item, SemaphoreSlim semaphore)
    {
        try
        {
            logger.LogInformation("Starting to process image: {FileName}", item.FileName);
            using var scope = serviceScopeFactory.CreateScope();
            var imageProcessingService = scope.ServiceProvider.GetRequiredService<IImageProcessingService>();

            await imageProcessingService.ProcessImageAsync(item.Stream, item.FileName, item.Id);
            logger.LogInformation("Successfully processed image: {FileName} with ID: {Id}", item.FileName, item.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing image: {FileName} with ID: {Id}", item.FileName, item.Id);
        }
        finally
        {
            await item.Stream.DisposeAsync();
            semaphore.Release();
            logger.LogInformation("Released processing slot for: {FileName}", item.FileName);
        }
    }
}