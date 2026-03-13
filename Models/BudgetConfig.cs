namespace FinanceAnalyzerPro.Models;

public class BudgetConfig
{
    public Dictionary<string, decimal> CategoryBudgets { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}