namespace _Tripfinity.Interfaces;

public interface ISessionStore
{
    void Set(string token);
    Utilities.Sessions Get();
}