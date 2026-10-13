using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BankAccounts.Application.Outbox;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using BankAccounts.Application.Abstractions.Interfaces;

namespace BankAccounts.Infrastructure.Messaging;

public sealed class SnsEventPublisher: IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _simpleNotificationService;
    private readonly IOptions<SnsOptions> _options;
    private readonly ILogger<SnsEventPublisher> _logger;
    public SnsEventPublisher(
        IAmazonSimpleNotificationService simpleNotificationService,
        IOptions<SnsOptions> options,
        ILogger<SnsEventPublisher> logger
        )
    {
        _simpleNotificationService = simpleNotificationService;
        _options = options;
        _logger = logger;
    }

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var request = new PublishRequest
        {
            TopicArn = _options.Value.TopicArn,
            Message = message.Payload,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["eventType"] = new() { DataType = "String", StringValue = message.EventType },
                ["eventId"] = new() { DataType = "String", StringValue = message.MessageId.ToString() }
            }
        };

        var response = await _simpleNotificationService.PublishAsync(request, cancellationToken);

        _logger.LogInformation(
            "Published {EventType} message {MessageId} to SNS as {SnsMessageId}",
            message.EventType, message.MessageId, response.MessageId);
    }
}
