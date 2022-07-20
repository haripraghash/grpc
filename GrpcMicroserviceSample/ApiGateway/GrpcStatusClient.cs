using Grpc.Net.Client;
using Status;

namespace ApiGateway;

public class GrpcStatusClient : IGrpcStatusClient
{
    private readonly GrpcChannel _channel;
    private  readonly StatusManager.StatusManagerClient _client;

    public GrpcStatusClient(string serverUrl)
    {
        _channel = GrpcChannel.ForAddress(serverUrl);
        _client = new StatusManager.StatusManagerClient(_channel);
    }
    
    public async Task<IEnumerable<ClientStatusModel>> GetAllStatuses()
    {
        var statuses = new List<ClientStatusModel>();
        using var call = _client.GetAllStatuses(new ClientStatusesRequest());

        while (await call.ResponseStream.MoveNext(new CancellationToken()))
        {
            var currentStatus = call.ResponseStream.Current;
            statuses.Add(new ClientStatusModel
            {
                Name = currentStatus.ClientName,
                Status = (ClientStatus)currentStatus.Status
            });
        }

        return statuses;
    }

    public async Task<ClientStatusModel> GetClientStatus(string clientName)
    {
        var call =  _client.GetClientStatus(new ClientStatusRequest { ClientName = clientName });
        return await Task.FromResult(new ClientStatusModel()
        {
            Name = call.ClientName,
            Status = (ClientStatus)call.Status  
        });
    }

    public async Task<bool> UpdateClientStatus(string clientName, ClientStatus status)
    {
        var call = await _client.UpdateClientStatusAsync(new ClientStatusUpdateRequest() { ClientName = clientName, Status = (Status.ClientStatus)status });
        return call.Success;
    }
    
    public void Dispose()
    {
        _channel.Dispose();
    }
}