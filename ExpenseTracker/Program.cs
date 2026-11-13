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
app.MapGet("/expenses/{id}", async (int id, AppDbContext db) => 
  await db.Expenses.FindAsync(id) is Expense expense 
  ? Results.Ok(expense)
  : Results.NotFound());
app.MapPut("/expenses/{id}", async (int id, Expense input, AppDbContext db) =>
{
  var expense = await db.Expenses.FindAsync(id);
  if (expense is null) return Results.NotFound();

  expense.Amount = input.Amount;
  expense.Date = input.Date;
  expense.Description = input.Description;
  expense.Category = input.Category;

  await db.SaveChangesAsync();
  return Results.Ok(expense);
});
app.MapDelete("expenses/{id}", async (int id, AppDbContext db) =>
{
  var expense = await db.Expenses.FindAsync(id);
  if (expense is null) return Results.NotFound();

  db.Expenses.Remove(expense);
  await db.SaveChangesAsync();
  return Results.NoContent();
});

app.MapGet("/", () => "Expense Tracker API is running");
app.Run();