namespace FinanceAnalyzerPro.Models;

public class MerchantInsight
{
    public string Merchant { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public string PrimaryCategory { get; set; } = string.Empty;
}