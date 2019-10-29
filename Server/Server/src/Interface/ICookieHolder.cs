namespace Server.Interface
{
    public interface ICookieHolder
    {
        int NowCookieCount { get; }
        int AddCookie();
    }
}
