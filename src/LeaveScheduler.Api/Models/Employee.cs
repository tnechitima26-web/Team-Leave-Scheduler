namespace LeaveScheduler.Api.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty; // Engineering, Operations, or Finance
}