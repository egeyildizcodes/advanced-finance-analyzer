namespace FinanceAnalyzerPro.Models;

public class FinancialHealth
{
    public int CashflowScore { get; set; }
    public int SavingsScore { get; set; }
    public int BudgetDisciplineScore { get; set; }
    public int RiskScore { get; set; }
    public int OverallScore { get; set; }
    public string Status { get; set; } = string.Empty;
}