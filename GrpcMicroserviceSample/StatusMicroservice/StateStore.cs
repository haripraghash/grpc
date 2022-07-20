namespace StatusMicroservice;

public interface IStateStore
{
    IEnumerable<(string ClientName, ClientStatus clientStatus)> GetAllStatuses();
    bool UpdateStatus(string clientName, ClientStatus clientStatus);
    ClientStatus GetStatus(string clientName);
}

public class StateStore : IStateStore
{
    private Dictionary<string, ClientStatus> _statuses = new Dictionary<string, ClientStatus>();
    
    public IEnumerable<(string ClientName, ClientStatus clientStatus)> GetAllStatuses()
    {
       var returnedStatuses = new List<(string ClientName, ClientStatus clientStatus)>();
       
       foreach(var status in _statuses)
       {
           returnedStatuses.Add((status.Key, status.Value));
       }

       return returnedStatuses;
    }

    public bool UpdateStatus(string clientName, ClientStatus clientStatus)
    {
        _statuses[clientName] = clientStatus;
        return true;
    }

    public ClientStatus GetStatus(string clientName)
    {
        if (!_statuses.ContainsKey(clientName))
        {
            return ClientStatus.OFFLINE;
        }

        return _statuses[clientName];
    }
}