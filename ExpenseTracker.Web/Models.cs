public record ExpenseDto(int Id, decimal Amount, DateOnly Date, string Description, int CategoryId, string Category);
public record CategoryDto(int Id, string Name);
public record ValidationProblem(Dictionary<string, string[]> Errors);