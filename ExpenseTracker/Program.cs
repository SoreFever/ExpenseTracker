using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=expenses.db"));

builder.Services.AddCors(options =>
{
  options.AddPolicy("Frontend", policy => 
    policy.WithOrigins("http://localhost:5200")
      .AllowAnyHeader()
      .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("Frontend");

app.MapGet("/", () => "Expense Tracker API is running");

// Categories
app.MapGet("/categories", async (AppDbContext db) =>
  await db.Categories
      .OrderBy(c => c.Name)
      .Select(c => new CategoryDto(c.Id, c.Name))
      .ToListAsync());

app.MapPost("/categories", async (CategoryInput input, AppDbContext db) =>
{
  var name = input.Name?.Trim() ?? "";
  var errors = new Dictionary<string, string[]>();

  if (name.Length == 0) 
    errors["name"] = ["Name is required."];
  else if (name.Length > 50)
    errors["name"] = ["Name must be 50 characters or fewer."];
  else if (await db.Categories.AnyAsync(c => c.Name == name))
    errors["name"] = ["That category already exists."];
  
  if (errors.Count > 0) return Results.ValidationProblem(errors);

  var category = new Category { Name = name };
  db.Categories.Add(category);
  await db.SaveChangesAsync();
  return Results.Created($"/categories/{category.Id}", new CategoryDto(category.Id, category.Name));
});

// Expenses

app.MapGet("/expenses", async (AppDbContext db) => 
  await db.Expenses
    .OrderByDescending(e => e.Date)
    .Select(e => new ExpenseDto(e.Id, e.Amount, e.Date, e.Description, e.CategoryId, e.Category!.Name))
    .ToListAsync());

app.MapGet("/expenses/{id}", async (int id, AppDbContext db) =>
{
  var expense = await db.Expenses.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id);
  return expense is null ? Results.NotFound() : Results.Ok(ToDto(expense, expense.Category!.Name));
});

app.MapPost("/expenses", async (ExpenseInput input, AppDbContext db) =>
{
  var category = await db.Categories.FindAsync(input.CategoryId);
  var errors = ValidateExpense(input, category);

  if (string.IsNullOrWhiteSpace(input.Description))
    errors["description"] = ["Description is required."];
  if (errors.Count > 0) return Results.ValidationProblem(errors);

  var expense = new Expense
  {
    Amount = input.Amount,
    Date = input.Date,
    Description = input.Description.Trim(),
    CategoryId = input.CategoryId
  };

  db.Expenses.Add(expense);
  await db.SaveChangesAsync();
  return Results.Created($"/expenses/{expense.Id}", ToDto(expense, category!.Name));
});

app.MapPut("/expenses/{id}", async (int id, ExpenseInput input, AppDbContext db) =>
{
  var expense = await db.Expenses.FindAsync(id);
  if (expense is null) return Results.NotFound();

  var category = await db.Categories.FindAsync(input.CategoryId);
  var errors = ValidateExpense(input, category);
  if (errors.Count > 0) return Results.ValidationProblem(errors);

  expense.Amount = input.Amount;
  expense.Date = input.Date;
  expense.Description = input.Description.Trim();
  expense.CategoryId = input.CategoryId;

  await db.SaveChangesAsync();
  return Results.Ok(ToDto(expense, category!.Name));
});

app.MapDelete("expenses/{id}", async (int id, AppDbContext db) =>
{
  var expense = await db.Expenses.FindAsync(id);
  if (expense is null) return Results.NotFound();

  db.Expenses.Remove(expense);
  await db.SaveChangesAsync();
  return Results.NoContent();
});

// Summary

app.MapGet("/summary", async (string month, AppDbContext db) =>
{
  if (!DateOnly.TryParseExact($"{month}-01", "yyyy-MM-dd", out var start))
    return Results.BadRequest("Month must look like 2026-11.");
  
  var end = start.AddMonths(1);

  var rows = await db.Expenses
    .Where(e => e.Date >= start && e.Date < end)
    .Select(e => new {e.Amount, Category = e.Category!.Name})
    .ToListAsync();
  
  var byCategory = rows
    .GroupBy(r => r.Category)
    .Select(g => new CategoryTotal(g.Key, g.Sum(r => r.Amount)))
    .OrderByDescending(t => t.Total)
    .ToList();
  
  return Results.Ok(new SummaryDto(month, rows.Sum(r => r.Amount), byCategory));
});

app.Run();

// Helpers
static ExpenseDto ToDto(Expense e, string categoryName) => 
  new(e.Id, e.Amount, e.Date, e.Description, e.CategoryId, categoryName);

static Dictionary<string, string[]> ValidateExpense(ExpenseInput input, Category? category)
{
  var errors = new Dictionary<string, string[]>();

  if (input.Amount <= 0)
    errors["amount"] = ["Amount must be greater than zero."];
  if (input.Date == default)
    errors["date"] = ["Date is required."];
  if (category is null)
    errors["categoryId"] = ["Category does not exists."];
  
  return errors;
}