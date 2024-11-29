public class JobLog
{
    public int Id { get; set; }
    public required string JobName { get; set; }
    public required string Status { get; set; }
    public required string Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
