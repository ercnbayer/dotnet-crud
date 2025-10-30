using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using dotnetcrud.Repositories;
using File = dotnetcrud.Model.File;
using Microsoft.Extensions.DependencyInjection;

namespace dotnetcrud.S3;

public class S3EventWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAmazonSQS _sqs;

    public S3EventWorker(IAmazonSQS sqs, IServiceScopeFactory scopeFactory)
    {
        _sqs = sqs;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) // TO DO FIX IT AND DO PROPER LOGGING
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
            var config = scope.ServiceProvider.GetRequiredService<S3Config>();

            // --- Wait for queue to exist ---
            string queueUrl = null;
            while (queueUrl == null && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var queueUrlResponse = await _sqs.GetQueueUrlAsync(config.QueueName, stoppingToken);
                    queueUrl = queueUrlResponse.QueueUrl;
                }
                catch
                {
                    await Task.Delay(2000, stoppingToken);
                }
            }

            // --- Receive messages ---
            var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                WaitTimeSeconds = 5,
                MaxNumberOfMessages = 10
            }, stoppingToken);

            foreach (var msg in response.Messages)
            {
                try
                {
                    var s3Event = JsonDocument.Parse(msg.Body);
                    var recordKey = s3Event.RootElement
                        .GetProperty("Records")[0]
                        .GetProperty("s3")
                        .GetProperty("object")
                        .GetProperty("key")
                        .GetString();

                    if (recordKey == null) throw new Exception("Record key not found");

                    var segments = recordKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    var userId = segments.Length > 1 ? segments[1] : null;

                    if (userId == null) throw new Exception("User id not found in path segments");

                    var fileName = Path.GetFileName(recordKey);

                    var nameParts = fileName.Split('-');
                    var nameWithoutTimestamp = nameParts[0];
                    var extension = Path.GetExtension(nameWithoutTimestamp);

                    if (string.IsNullOrEmpty(extension)) throw new Exception("Extension not found");

                    string contentType = extension.ToLowerInvariant() switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".pdf" => "application/pdf",
                        ".txt" => "text/plain",
                        ".json" => "application/json",
                        ".csv" => "text/csv",
                        _ => "application/octet-stream"
                    };

                    var file = new File
                    {
                        BucketName = config.BucketName,
                        FileName = fileName,
                        UserId = Guid.Parse(userId),
                        ContentType = contentType
                    };

                    var result = await fileRepository.CreateAsync(file);
                    Console.WriteLine(result);
                    if (!result.IsSuccess)
                        Console.WriteLine($"Failed to create file record for {fileName}. Error: {result.Error}");
                    else
                        await _sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing SQS message: {ex.Message}");
                }
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}
