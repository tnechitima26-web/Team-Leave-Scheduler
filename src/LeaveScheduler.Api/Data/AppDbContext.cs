using Microsoft.EntityFrameworkCore;
using LeaveScheduler.Api.Models;

namespace LeaveScheduler.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<PublicHoliday> PublicHolidays { get; set; }
}