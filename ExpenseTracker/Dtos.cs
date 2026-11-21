public record ExpenseInput(decimal Amount, DateOnly Date, string Description, int CategoryId);
public record ExpenseDto(int Id, decimal Amount, DateOnly Date, string Description, int CategoryId, string Category);

public record CategoryInput(string Name);
public record CategoryDto(int Id, string Name);

public record CategoryTotal(string Category, decimal Total);
public record SummaryDto(string Month, decimal Total, List<CategoryTotal> ByCategory);