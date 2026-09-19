using Grpc.Core;
using InterviewHub.Api.Grpc;
using InterviewHub.Data;

namespace InterviewHub.Api.Services;

public class ProgressGrpcService(IQuestionRepository repository, QuestionNotifier notifier)
    : ProgressService.ProgressServiceBase
{
    public override async Task<CountResponse> GetQuestionCount(CountRequest request, ServerCallContext context)
    {
        var questions = await repository.GetQuestions();
        return new CountResponse { Total = questions.Count };
    }

    public override async Task WatchNewQuestions(
        WatchRequest request,
        IServerStreamWriter<QuestionNotification> responseStream,
        ServerCallContext context)
    {
        await foreach (var notification in notifier.ReadAllAsync(context.CancellationToken))
        {
            await responseStream.WriteAsync(notification);
        }
    }
}