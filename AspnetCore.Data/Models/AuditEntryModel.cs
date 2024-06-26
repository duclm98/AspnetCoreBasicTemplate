using AspnetCore.Data.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace AspnetCore.Data.Models;

public class AuditEntryModel
{
    public AuditEntryModel(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string Method { get; set; }
    public int? CreatedUserId { get; set; }
    public AuditLogType Type { get; set; }
    public string TableName { get; set; }
    public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
    public List<string> ChangedColumns { get; } = new List<string>();
    public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

    public bool HasTemporaryProperties => TemporaryProperties.Any();

    public AuditLog ToAudit()
    {
        var changedColumnRemoveDuplicate = ChangedColumns.Distinct().ToList();
        return new AuditLog
        {
            CreatedUserId = CreatedUserId,
            Type = Type,
            TableName = TableName,
            PrimaryKey = JsonConvert.SerializeObject(KeyValues),
            OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
            NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
            ColumnName = changedColumnRemoveDuplicate.Count == 0
                ? null : JsonConvert.SerializeObject(changedColumnRemoveDuplicate)
        };
    }
}