namespace AspnetCore.Utilities.AppsettingVariables;

public class EnvironmentVariable
{
    public string LoggingFolder { get; set; }
    public string JwtIssuer { get; set; }
    public string JwtAudience { get; set; }
    public string JwtAccessTokenSecret { get; set; }
    public int JwtAccessTokenExpires { get; set; }
    public string JwtRefreshTokenSecret { get; set; }
    public int JwtRefreshTokenExpires { get; set; }
}