namespace ApiGateway;

public interface IGrpcPerformanceClient
{
    Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatusAsync(string clientName);
    Task<IEnumerable<ResponseModel.PerformanceStatusModel>> GetManyPerformanceStatusAsync(IEnumerable<string> clientNames);
}