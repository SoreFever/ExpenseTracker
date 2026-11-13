using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=expenses.db"));

var app = builder.Build();
app.MapGet("/", () => "Expense Tracker API is running");
app.Run();