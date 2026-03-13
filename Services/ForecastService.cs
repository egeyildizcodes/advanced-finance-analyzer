using FinanceAnalyzerPro.Models;

namespace FinanceAnalyzerPro.Services;

public class ForecastService
{
    public decimal ForecastNextMonthExpense(Dictionary<string, MonthlyMetrics> monthlyTrend)
    {
        var expenseSeries = monthlyTrend
            .OrderBy(x => x.Key)
            .Select(x => x.Value.Expense)
            .ToList();

        if (expenseSeries.Count == 0)
            return 0m;

        if (expenseSeries.Count == 1)
            return expenseSeries[0];

        if (expenseSeries.Count == 2)
            return Math.Round((expenseSeries[0] + expenseSeries[1]) / 2m, 2);

        // weighted moving average: newest months matter more
        decimal totalWeight = 0;
        decimal weightedSum = 0;

        int weight = 1;
        foreach (var value in expenseSeries)
        {
            weightedSum += value * weight;
            totalWeight += weight;
            weight++;
        }

        return Math.Round(weightedSum / totalWeight, 2);
    }
}