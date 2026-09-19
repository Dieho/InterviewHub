using System.Threading.Channels;
using InterviewHub.Api.Grpc;

namespace InterviewHub.Api.Services;

public class QuestionNotifier
{
    private readonly Channel<QuestionNotification> _channel =
        Channel.CreateUnbounded<QuestionNotification>();

    public async Task PublishAsync(QuestionNotification notification)
    {
        await _channel.Writer.WriteAsync(notification);
    }

    public IAsyncEnumerable<QuestionNotification> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}