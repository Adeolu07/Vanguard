
using _Tripfinity.Interfaces;

namespace _Tripfinity.Utilities;

public class SessionStore : ISessionStore
{
    private Sessions? _session;

    public void Set(string token)
    {
        _session = new Sessions{Token = token};
    }

    public Sessions Get() => _session;

}