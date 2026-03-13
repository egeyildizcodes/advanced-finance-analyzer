namespace FinanceAnalyzerPro.Models;

public class AnalysisResult
{
    public int TransactionCount { get; set; }
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetSavings { get; set; }
    public decimal SavingsRate { get; set; }

    public decimal AverageMonthlyIncome { get; set; }
    public decimal AverageMonthlyExpense { get; set; }
    public decimal ForecastNextMonthExpense { get; set; }

    public Dictionary<string, MonthlyMetrics> MonthlyTrend { get; set; } = new();
    public List<CategoryInsight> CategoryInsights { get; set; } = new();
    public List<MerchantInsight> TopMerchants { get; set; } = new();

    public List<string> BudgetAlerts { get; set; } = new();
    public List<string> Anomalies { get; set; } = new();
    public List<string> RecurringPayments { get; set; } = new();
    public List<Transaction> TopExpenses { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();

    public FinancialHealth FinancialHealth { get; set; } = new();
}