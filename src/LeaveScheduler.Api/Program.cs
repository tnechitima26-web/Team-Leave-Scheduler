using Microsoft.EntityFrameworkCore;
using LeaveScheduler.Api.Data;
using LeaveScheduler.Api.Services;
using LeaveScheduler.Api.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database and Logic Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=scheduler.db"));
builder.Services.AddScoped<LeaveService>();

// Add CORS so the Frontend can talk to the Backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// 1. Seed the database (Keep this here)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(context);
}

// 2. Configure Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. THIS ORDER IS CRITICAL:
app.UseHttpsRedirection();
app.UseCors("AllowAll"); // Move this right here, BEFORE app.MapGet/MapPost

// --- API ENDPOINTS ---

// 1. GET THE CALENDAR (Next 30 days)
app.MapGet("/api/leave/calendar", async (AppDbContext db) =>
{
    var today = DateTime.Today;
    var thirtyDaysFromNow = today.AddDays(30);

    return await db.LeaveRequests
        .Include(r => r.Employee)
        .Where(r => r.Status == "Approved" && r.StartDate <= thirtyDaysFromNow && r.EndDate >= today)
        .ToListAsync();
});

// 2. GET PENDING REQUESTS
app.MapGet("/api/leave/pending", async (AppDbContext db) =>
{
    return await db.LeaveRequests
        .Include(r => r.Employee)
        .Where(r => r.Status == "Pending")
        .ToListAsync();
});

// 3. SUBMIT A REQUEST
app.MapPost("/api/leave/request", async (AppDbContext db, LeaveService leaveService, [FromBody] LeaveRequest request) =>
{
    var validation = await leaveService.ValidateRequest(request);
    
    if (!validation.IsValid)
    {
        return Results.BadRequest(new { message = validation.Message });
    }

    request.Status = "Pending";
    db.LeaveRequests.Add(request);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Request submitted successfully" });
});

// 4. APPROVE OR REJECT A REQUEST
app.MapPost("/api/leave/{id}/status", async (AppDbContext db, int id, [FromBody] string newStatus) =>
{
    var request = await db.LeaveRequests.FindAsync(id);
    if (request == null) return Results.NotFound();

    request.Status = newStatus;
    await db.SaveChangesAsync();

    return Results.Ok(new { message = $"Request {newStatus}" });
});

app.Run();