using Grpc.Net.Client;
using Worker;

namespace ApiGateway;

public class GrpcJobsClient : IGrpcJobsClient, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly JobManager.JobManagerClient _client;
    
    public GrpcJobsClient(string serverUrl)
    {
        _channel = GrpcChannel.ForAddress(serverUrl);
        _client = new JobManager.JobManagerClient(_channel);
    }

    public async Task SendJobs(IEnumerable<JobModel> jobs)
    {
        using var call = this._client.SendJobs();
        foreach (var job in jobs)
        {
            await call.RequestStream.WriteAsync(
                new SendJobsRequest{JobId = job.JobId, JobDescription = job.JobDescription});
        }

        await call.RequestStream.CompleteAsync();
        await call;
    }

    public async Task TriggerJobs(int jobCount)
    {
        using var call = this._client.TriggerJobs(new TriggerJobsRequest{JobsCount = jobCount});
        while (await call.ResponseStream.MoveNext(new CancellationToken()))
        {
            Console.WriteLine($"" +
                              $"JobSequence: {call.ResponseStream.Current.JobSequence}, " +
                              $"JobDescription: {call.ResponseStream.Current.JobMessage}");

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    public void Dispose()
    {
        this._channel.Dispose();
    }
}