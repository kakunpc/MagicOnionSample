using System.Threading.Tasks;
using MagicOnion;

namespace Protocol
{
    /// <summary>
    /// Server -> Client
    /// </summary>
    public interface IRoomHubReceiver
    {
        void OnJoin(string name);
        
        void OnLeave(string name);
        
        void OnChangeCookieCount(int value);
    }
    
    /// <summary>
    /// client -> Server
    /// </summary>
    public interface IRoomHub: IStreamingHub<IRoomHub, IRoomHubReceiver>
    {
        Task JoinAsync(string userName);
        
        Task LeaveAsync();
        
        Task AddCookieAsync();

        Task<int> GetCookieAsync();
    }
}
