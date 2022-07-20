using Grpc.Core;
using Status;

namespace StatusMicroservice.Services;

public class StatusManagerService : StatusManager.StatusManagerBase
{
    private readonly IStateStore _stateStore;

    public StatusManagerService(IStateStore stateStore)
    {
        _stateStore = stateStore;
    }

    public override async Task GetAllStatuses(ClientStatusesRequest request,
        IServerStreamWriter<ClientStatusResponse> responseStream, ServerCallContext context)
    {
        foreach (var record in _stateStore.GetAllStatuses())
        {
            await responseStream.WriteAsync(new ClientStatusResponse
            {
                Status = (Status.ClientStatus) record.clientStatus,
                ClientName = record.ClientName
            });
        }
    }

    public override Task<ClientStatusResponse> GetClientStatus(ClientStatusRequest request, ServerCallContext context)
    {
        return Task.FromResult(new ClientStatusResponse()
        {
            Status = (Status.ClientStatus) _stateStore.GetStatus(request.ClientName),
            ClientName = request.ClientName
        });
    }

    public override Task<ClientStatusUpdateResponse> UpdateClientStatus(ClientStatusUpdateRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new ClientStatusUpdateResponse()
        {
            Success = _stateStore.UpdateStatus(request.ClientName, (ClientStatus) request.Status)
        });
    }
}