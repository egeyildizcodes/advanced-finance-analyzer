using FinanceAnalyzerPro.Models;

namespace FinanceAnalyzerPro.Services;

public class RecommendationEngine
{
    public List<string> GenerateRecommendations(AnalysisResult result)
    {
        var recommendations = new List<string>();

        if (result.SavingsRate < 10)
        {
            recommendations.Add("Savings rate is low. Reduce discretionary categories such as Food, Entertainment, and Shopping.");
        }

        if (result.BudgetAlerts.Count > 0)
        {
            recommendations.Add("One or more categories exceeded budget. Review categories with repeated overspending and reduce monthly caps exposure.");
        }

        var topCategory = result.CategoryInsights
            .OrderByDescending(x => x.TotalExpense)
            .FirstOrDefault();

        if (topCategory is not null)
        {
            recommendations.Add(
                $"Highest expense category is '{topCategory.Category}'. It accounts for {topCategory.PercentageOfTotalExpense:F2}% of total expenses.");
        }

        if (result.Anomalies.Count >= 2)
        {
            recommendations.Add("Multiple anomalies detected. Review unusually large purchases and confirm whether they are planned or one-off events.");
        }

        if (result.ForecastNextMonthExpense > result.AverageMonthlyIncome && result.AverageMonthlyIncome > 0)
        {
            recommendations.Add("Forecast suggests next month's expenses may exceed average monthly income. Preemptively cut variable spending.");
        }

        if (result.TopMerchants.Count > 0)
        {
            var merchant = result.TopMerchants.First();
            recommendations.Add(
                $"Top merchant spending is concentrated at '{merchant.Merchant}'. Consider whether this vendor represents recurring non-essential spending.");
        }

        if (result.RecurringPayments.Count > 0)
        {
            recommendations.Add("Recurring payments detected. Audit subscriptions and repeated bills for cancellation or renegotiation opportunities.");
        }

        if (result.FinancialHealth.OverallScore >= 85)
        {
            recommendations.Add("Financial profile is strong. Consider redirecting excess cash flow into investments or emergency fund growth.");
        }
        else if (result.FinancialHealth.OverallScore < 50)
        {
            recommendations.Add("Financial risk profile is elevated. Prioritize cash preservation, budget compliance, and expense reduction immediately.");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("No major issues detected. Continue monitoring spending consistency and maintain current budget discipline.");
        }

        return recommendations;
    }
}