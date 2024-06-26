namespace AspnetCore.Data.Models;

public class AuditLogCreatingModel
{
    public int? CreatedUserId { get; set; }
    public string Method { get; set; }
}