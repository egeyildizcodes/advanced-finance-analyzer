namespace FinanceAnalyzerPro.Models;

public class CategoryInsight
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalExpense { get; set; }
    public decimal AverageExpense { get; set; }
    public int Count { get; set; }
    public decimal PercentageOfTotalExpense { get; set; }
}