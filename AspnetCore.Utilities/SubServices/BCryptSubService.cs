namespace AspnetCore.Utilities.SubServices;

public interface IBCryptSubService
{
    string HashPassword(string password);
    bool IsMatchPssword(string hash, string password);
}

public class BCryptSubService : IBCryptSubService
{
    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool IsMatchPssword(string hash, string password) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}