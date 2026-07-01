using Microsoft.EntityFrameworkCore;
using LeaveScheduler.Api.Data;
using LeaveScheduler.Api.Models;
using LeaveScheduler.Api.Services;
using Xunit;

namespace LeaveScheduler.Tests;

public class LeaveServiceTests
{
    private AppDbContext GetDbContext()
    {
        // We use an "InMemory" database for testing so we don't mess up our actual data
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task OverlapRule_ShouldReject_IfEmployeeAlreadyHasApprovedLeave()
    {
        // Arrange (Setup the scenario)
        var context = GetDbContext();
        var service = new LeaveService(context);

        context.Employees.Add(new Employee { Id = 1, Name = "Test User", Team = "Engineering" });
        context.LeaveRequests.Add(new LeaveRequest 
        { 
            EmployeeId = 1, 
            StartDate = new DateTime(2025, 1, 10), 
            EndDate = new DateTime(2025, 1, 12), 
            Status = "Approved" 
        });
        await context.SaveChangesAsync();

        var newRequest = new LeaveRequest 
        { 
            EmployeeId = 1, 
            StartDate = new DateTime(2025, 1, 11), // This overlaps!
            EndDate = new DateTime(2025, 1, 15) 
        };

        // Act (Run the logic)
        var result = await service.ValidateRequest(newRequest);

        // Assert (Check if it failed as expected)
        Assert.False(result.IsValid);
        Assert.Contains("overlap", result.Message.ToLower());
    }

    [Fact]
    public async Task ThirtyPercentRule_ShouldReject_IfTeamCapacityIsExceeded()
    {
        // Arrange: A team of 4 means only 1 person can be off (30% of 4 = 1.2, rounded = 1)
        var context = GetDbContext();
        var service = new LeaveService(context);

        context.Employees.AddRange(new[] {
            new Employee { Id = 1, Name = "User 1", Team = "Engineering" },
            new Employee { Id = 2, Name = "User 2", Team = "Engineering" },
            new Employee { Id = 3, Name = "User 3", Team = "Engineering" },
            new Employee { Id = 4, Name = "User 4", Team = "Engineering" }
        });

        // Already 1 person approved for Jan 20th
        context.LeaveRequests.Add(new LeaveRequest { 
            EmployeeId = 1, 
            StartDate = new DateTime(2025, 1, 20), 
            EndDate = new DateTime(2025, 1, 20), 
            Status = "Approved" 
        });
        await context.SaveChangesAsync();

        // A 2nd person tries to book Jan 20th
        var secondRequest = new LeaveRequest { 
            EmployeeId = 2, 
            StartDate = new DateTime(2025, 1, 20), 
            EndDate = new DateTime(2025, 1, 20) 
        };

        // Act
        var result = await service.ValidateRequest(secondRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("capacity exceeded", result.Message.ToLower());
    }
}