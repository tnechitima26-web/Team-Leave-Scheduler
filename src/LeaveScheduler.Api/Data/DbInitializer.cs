using System.Globalization;
using System.Text.Json;
using LeaveScheduler.Api.Models;

namespace LeaveScheduler.Api.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Create the database if it doesn't exist
        context.Database.EnsureCreated();

        // 1. Seed Employees if empty
        if (!context.Employees.Any())
        {
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "employees.csv");
            var lines = File.ReadAllLines(csvPath).Skip(1); // Skip the header row
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                context.Employees.Add(new Employee { Name = parts[1], Team = parts[2] });
            }
        }

        // 2. Seed Public Holidays if empty
        if (!context.PublicHolidays.Any())
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "public_holidays.json");
            var jsonString = File.ReadAllText(jsonPath);
            var holidays = JsonSerializer.Deserialize<List<PublicHoliday>>(jsonString);
            if (holidays != null)
            {
                context.PublicHolidays.AddRange(holidays);
            }
        }

        context.SaveChanges();
    }
}