using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=expenses.db"));

var app = builder.Build();

app.MapPost("/expenses", async (Expense expense, AppDbContext db) =>
{
  db.Expenses.Add(expense);
  await db.SaveChangesAsync();
  return Results.Created($"/expenses/{expense.Id}", expense);
});
app.MapGet("/expenses", async (AppDbContext db) => 
  await db.Expenses.OrderByDescending(e => e.Date).ToListAsync());

app.MapGet("/", () => "Expense Tracker API is running");
app.Run();