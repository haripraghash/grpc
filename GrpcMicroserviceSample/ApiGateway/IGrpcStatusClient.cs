namespace ApiGateway;

public interface IGrpcStatusClient
{
    Task<IEnumerable<ClientStatusModel>> GetAllStatuses();
    Task<ClientStatusModel> GetClientStatus(string clientName);
    Task<bool> UpdateClientStatus(string clientName, ClientStatus status);
}