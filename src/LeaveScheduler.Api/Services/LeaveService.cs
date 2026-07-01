using Microsoft.EntityFrameworkCore;
using LeaveScheduler.Api.Data;
using LeaveScheduler.Api.Models;

namespace LeaveScheduler.Api.Services;

public class LeaveService
{
    private readonly AppDbContext _context;

    public LeaveService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsValid, string Message)> ValidateRequest(LeaveRequest request)
    {
        // 1. Get the Employee and their Team
        var employee = await _context.Employees.FindAsync(request.EmployeeId);
        if (employee == null) return (false, "Employee not found.");

        // 2. CHECK OVERLAP: Is this employee already approved for these dates?
        var overlapping = await _context.LeaveRequests
            .AnyAsync(r => r.EmployeeId == request.EmployeeId && 
                           r.Status == "Approved" &&
                           request.StartDate <= r.EndDate && 
                           r.StartDate <= request.EndDate);

        if (overlapping) return (false, "You already have an approved request that overlaps these dates.");

        // 3. GET TEAM INFO
        var teamMembers = await _context.Employees.Where(e => e.Team == employee.Team).ToListAsync();
        var teamSize = teamMembers.Count;
        
        // RULE: 30% rounded to nearest whole, minimum 1.
        int maxAllowedOff = (int)Math.Max(1, Math.Round(teamSize * 0.3));

        // 4. PER-DAY CHECK: Loop through every day in the requested range
        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            // Skip Weekends (Saturday = 6, Sunday = 0)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

            // Skip Public Holidays
            var isHoliday = await _context.PublicHolidays.AnyAsync(h => h.Date.Date == date.Date);
            if (isHoliday) continue;

            // Count how many people from this team are ALREADY approved for THIS specific day
            var countAlreadyOff = await _context.LeaveRequests
                .CountAsync(r => r.Employee!.Team == employee.Team &&
                                 r.Status == "Approved" &&
                                 date >= r.StartDate && date <= r.EndDate);

            if (countAlreadyOff >= maxAllowedOff)
            {
                return (false, $"Team capacity exceeded on {date:yyyy-MM-dd}. Only {maxAllowedOff} person/people can be off.");
            }
        }

        return (true, "Request is valid.");
    }
}