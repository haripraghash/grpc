using Grpc.Core;
using Grpc.Net.Client;
using Performance;
using Monitor = Performance.Monitor;

namespace ApiGateway;

internal class GrpcPerformanceClient : IGrpcPerformanceClient, IDisposable
{
    private GrpcChannel _channel;
    
    public GrpcPerformanceClient(string serverUrl)
    {
        _channel = GrpcChannel.ForAddress(serverUrl);
    }
    
    public async Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatusAsync(string clientName)
    {
        var monitorClient = new Monitor.MonitorClient(_channel);
        var response = await monitorClient.GetPerformanceAsync(new PerformanceStatusRequest()
        {
            ClientName = clientName
        });

        return new ResponseModel.PerformanceStatusModel()
        {
            CpuPercentageUsage = response.CpuPercentageUsage,
            MemoryUsage = response.MemoryUsage,
            ActiveConnections = response.ActiveConnections,
            ProcessesRunning = response.ProcessesRunning
        };
    }

    public async Task<IEnumerable<ResponseModel.PerformanceStatusModel>> GetManyPerformanceStatusAsync(IEnumerable<string> clientNames)
    {
        var client = new Monitor.MonitorClient(_channel);
        using var call = client.GetManyPerformanceStats();
        var responses = new List<ResponseModel.PerformanceStatusModel>();

        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                responses.Add(new ResponseModel.PerformanceStatusModel()
                {
                    CpuPercentageUsage = response.CpuPercentageUsage,
                    MemoryUsage = response.MemoryUsage,
                    ActiveConnections = response.ActiveConnections,
                    ProcessesRunning = response.ProcessesRunning
                });
            }
        });

        foreach (var clientName in clientNames)
        {
            await call.RequestStream.WriteAsync(new PerformanceStatusRequest()
            {
                ClientName = clientName
            });
        }

        return responses;
    }

    public void Dispose()
    {
       this._channel.Dispose();
    }
}