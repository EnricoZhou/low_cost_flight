namespace low_cost_flight.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public string? UserName { get; set; }
    public string AuditData { get; set; } = string.Empty;
}
