namespace ApiGateway;

public interface IGrpcJobsClient
{
    Task SendJobs(IEnumerable<JobModel> jobs);
    Task TriggerJobs(int jobCount);
}