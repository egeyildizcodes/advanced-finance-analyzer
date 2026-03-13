namespace FinanceAnalyzerPro.Models;

public class Transaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Merchant { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;

    public bool IsIncome => Type.Equals("Income", StringComparison.OrdinalIgnoreCase);
    public bool IsExpense => Type.Equals("Expense", StringComparison.OrdinalIgnoreCase);
}