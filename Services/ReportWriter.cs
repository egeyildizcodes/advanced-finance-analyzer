using System.Text;
using System.Text.Json;
using FinanceAnalyzerPro.Models;

namespace FinanceAnalyzerPro.Services;

public static class ReportWriter
{
    public static void WriteTextReport(AnalysisResult result, string reportsDir)
    {
        Directory.CreateDirectory(reportsDir);

        var sb = new StringBuilder();
        sb.AppendLine("ADVANCED FINANCE ANALYZER PRO REPORT");
        sb.AppendLine(new string('=', 70));
        sb.AppendLine($"Generated At            : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Period                  : {result.MinDate:yyyy-MM-dd} -> {result.MaxDate:yyyy-MM-dd}");
        sb.AppendLine($"Transaction Count       : {result.TransactionCount}");
        sb.AppendLine($"Total Income            : {result.TotalIncome:C}");
        sb.AppendLine($"Total Expense           : {result.TotalExpense:C}");
        sb.AppendLine($"Net Cash Flow           : {result.NetSavings:C}");
        sb.AppendLine($"Savings Rate            : {result.SavingsRate:F2}%");
        sb.AppendLine($"Average Monthly Income  : {result.AverageMonthlyIncome:C}");
        sb.AppendLine($"Average Monthly Expense : {result.AverageMonthlyExpense:C}");
        sb.AppendLine($"Forecast Next Expense   : {result.ForecastNextMonthExpense:C}");
        sb.AppendLine();

        sb.AppendLine("FINANCIAL HEALTH");
        sb.AppendLine(new string('-', 70));
        sb.AppendLine($"Cashflow Score          : {result.FinancialHealth.CashflowScore}/100");
        sb.AppendLine($"Savings Score           : {result.FinancialHealth.SavingsScore}/100");
        sb.AppendLine($"Budget Discipline Score : {result.FinancialHealth.BudgetDisciplineScore}/100");
        sb.AppendLine($"Risk Score              : {result.FinancialHealth.RiskScore}/100");
        sb.AppendLine($"Overall Score           : {result.FinancialHealth.OverallScore}/100");
        sb.AppendLine($"Status                  : {result.FinancialHealth.Status}");
        sb.AppendLine();

        sb.AppendLine("CATEGORY INSIGHTS");
        sb.AppendLine(new string('-', 70));
        foreach (var item in result.CategoryInsights)
        {
            sb.AppendLine(
                $"{item.Category,-16} | Total: {item.TotalExpense,10:C} | Avg: {item.AverageExpense,10:C} | Count: {item.Count,3} | Share: {item.PercentageOfTotalExpense,6:F2}%");
        }
        sb.AppendLine();

        sb.AppendLine("MONTHLY TREND");
        sb.AppendLine(new string('-', 70));
        foreach (var month in result.MonthlyTrend.OrderBy(x => x.Key))
        {
            sb.AppendLine(
                $"{month.Key} | Income: {month.Value.Income:C} | Expense: {month.Value.Expense:C} | Net: {month.Value.Net:C}");
        }
        sb.AppendLine();

        sb.AppendLine("TOP MERCHANTS");
        sb.AppendLine(new string('-', 70));
        foreach (var merchant in result.TopMerchants)
        {
            sb.AppendLine(
                $"{merchant.Merchant,-20} | Total: {merchant.TotalAmount,10:C} | Count: {merchant.TransactionCount,3} | Category: {merchant.PrimaryCategory}");
        }
        sb.AppendLine();

        sb.AppendLine("BUDGET ALERTS");
        sb.AppendLine(new string('-', 70));
        if (result.BudgetAlerts.Count == 0)
            sb.AppendLine("No budget overruns.");
        else
            result.BudgetAlerts.ForEach(x => sb.AppendLine(x));
        sb.AppendLine();

        sb.AppendLine("ANOMALIES");
        sb.AppendLine(new string('-', 70));
        if (result.Anomalies.Count == 0)
            sb.AppendLine("No anomalies.");
        else
            result.Anomalies.ForEach(x => sb.AppendLine(x));
        sb.AppendLine();

        sb.AppendLine("RECURRING PAYMENTS");
        sb.AppendLine(new string('-', 70));
        if (result.RecurringPayments.Count == 0)
            sb.AppendLine("No recurring payment patterns detected.");
        else
            result.RecurringPayments.ForEach(x => sb.AppendLine(x));
        sb.AppendLine();

        sb.AppendLine("TOP EXPENSES");
        sb.AppendLine(new string('-', 70));
        foreach (var tx in result.TopExpenses)
        {
            sb.AppendLine(
                $"{tx.Date:yyyy-MM-dd} | {tx.Category,-14} | {tx.Merchant,-18} | {tx.Amount,10:C} | {tx.Description}");
        }
        sb.AppendLine();

        sb.AppendLine("RECOMMENDATIONS");
        sb.AppendLine(new string('-', 70));
        result.Recommendations.ForEach(x => sb.AppendLine($"- {x}"));

        File.WriteAllText(
            Path.Combine(reportsDir, "finance_report.txt"),
            sb.ToString(),
            Encoding.UTF8);
    }

    public static void WriteJsonReport(AnalysisResult result, string reportsDir)
    {
        Directory.CreateDirectory(reportsDir);

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(
            Path.Combine(reportsDir, "finance_report.json"),
            json,
            Encoding.UTF8);
    }
}