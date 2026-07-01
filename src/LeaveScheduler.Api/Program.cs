// 1. ALL "using" statements MUST be here at the top
using Microsoft.EntityFrameworkCore;
using LeaveScheduler.Api.Data;
using LeaveScheduler.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 2. Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// This is the Database connection we added in Commit 2
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=scheduler.db"));

builder.Services.AddScoped<LeaveService>();

var app = builder.Build();

// 3. Seed the database (the block we added in Commit 3)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(context);
}

// 4. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Basic starter endpoint (we will add more later)
app.MapGet("/ping", () => "Server is running!");

app.Run();