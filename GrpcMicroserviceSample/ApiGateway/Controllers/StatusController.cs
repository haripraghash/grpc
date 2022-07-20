using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase
{
    private readonly IGrpcStatusClient _grpcStatusClient;
    
    public StatusController(IGrpcStatusClient grpcStatusClient)
    {
        _grpcStatusClient = grpcStatusClient;
    }

    [HttpGet]
    public async Task<IEnumerable<ClientStatusModel>> GetAllStatuses()
    {
        return await _grpcStatusClient.GetAllStatuses();
    }
    
    [HttpGet("{clientName}")]
    public async Task<ClientStatusModel> GetStatus(string clientName)
    {
        return await _grpcStatusClient.GetClientStatus(clientName);
    }
    
    [HttpPatch("{clientName}/{status}")]
    public async Task<bool> UpdateStatus(string clientName, ClientStatus status)
    {
        return await _grpcStatusClient.UpdateClientStatus(clientName, status);
    }
   
}