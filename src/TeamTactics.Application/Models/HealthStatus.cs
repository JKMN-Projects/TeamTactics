namespace TeamTactics.Application.Models;

public class HealthStatus
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; }
    public DateTime CheckedAt { get; set; }
}
