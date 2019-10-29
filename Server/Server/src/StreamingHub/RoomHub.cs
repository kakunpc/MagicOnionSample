// BUG コンパイラが StreamingHubBase<IRoomHub, _> : IStreamingHub<IRoomHub, _> を正しく認識していない
// 詳細は以下をコメントアウトするとわかる
//#nullable disable

using System.Threading.Tasks;
using MagicOnion.Server.Hubs;
using Protocol;
using Server.Interface;

namespace Server.StreamingHub
{
    public sealed class RoomHub : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        private readonly ICookieHolder _cookieHolder;
        private IGroup _group;
        private string _name;

        public RoomHub(ICookieHolder cookieHolder)
        {
            _cookieHolder = cookieHolder;
        }

        public async Task JoinAsync(string userName)
        {
            const string roomName = "SampleRoom";
            _group = await Group.AddAsync(roomName);
            _name = userName;
            Broadcast(_group).OnJoin(userName);
        }

        public async Task LeaveAsync()
        {
            await _group.RemoveAsync(Context);
            Broadcast(_group).OnLeave(_name);
        }

        public Task AddCookieAsync()
        {
            var value =_cookieHolder.AddCookie();
            Broadcast(_group).OnChangeCookieCount(value);
            return Task.CompletedTask;
        }

        public async Task<int> GetCookieAsync()
        {
            await Task.CompletedTask;
            return _cookieHolder.NowCookieCount;
        }
    }
}
