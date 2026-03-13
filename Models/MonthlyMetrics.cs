namespace FinanceAnalyzerPro.Models;

public class MonthlyMetrics
{
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net => Income - Expense;
}