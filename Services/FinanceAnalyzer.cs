using FinanceAnalyzerPro.Models;

namespace FinanceAnalyzerPro.Services;

public class FinanceAnalyzer
{
    private readonly ForecastService _forecastService = new();
    private readonly ScoreCalculator _scoreCalculator = new();
    private readonly RecommendationEngine _recommendationEngine = new();

    public AnalysisResult Analyze(List<Transaction> transactions, Dictionary<string, decimal> budgets)
    {
        if (transactions == null || transactions.Count == 0)
        {
            throw new InvalidOperationException("No transactions found to analyze.");
        }

        var result = new AnalysisResult
        {
            TransactionCount = transactions.Count,
            MinDate = transactions.Min(t => t.Date),
            MaxDate = transactions.Max(t => t.Date),
            TotalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount),
            TotalExpense = transactions.Where(t => t.IsExpense).Sum(t => t.Amount)
        };

        result.NetSavings = result.TotalIncome - result.TotalExpense;
        result.SavingsRate = result.TotalIncome == 0
            ? 0
            : Math.Round((result.NetSavings / result.TotalIncome) * 100m, 2);

        result.MonthlyTrend = BuildMonthlyTrend(transactions);
        result.AverageMonthlyIncome = result.MonthlyTrend.Count == 0
            ? 0
            : Math.Round(result.MonthlyTrend.Values.Average(x => x.Income), 2);

        result.AverageMonthlyExpense = result.MonthlyTrend.Count == 0
            ? 0
            : Math.Round(result.MonthlyTrend.Values.Average(x => x.Expense), 2);

        result.ForecastNextMonthExpense = _forecastService.ForecastNextMonthExpense(result.MonthlyTrend);

        result.CategoryInsights = BuildCategoryInsights(transactions, result.TotalExpense);
        result.TopMerchants = BuildMerchantInsights(transactions);

        result.BudgetAlerts = DetectBudgetOverruns(result.CategoryInsights, budgets);
        result.Anomalies = DetectAnomalies(transactions);
        result.RecurringPayments = DetectRecurringPayments(transactions);

        result.TopExpenses = transactions
            .Where(t => t.IsExpense)
            .OrderByDescending(t => t.Amount)
            .Take(10)
            .ToList();

        result.FinancialHealth = _scoreCalculator.Calculate(
            result.TotalIncome,
            result.TotalExpense,
            result.SavingsRate,
            result.BudgetAlerts.Count,
            result.Anomalies.Count,
            result.ForecastNextMonthExpense,
            result.AverageMonthlyIncome);

        result.Recommendations = _recommendationEngine.GenerateRecommendations(result);

        return result;
    }

    private Dictionary<string, MonthlyMetrics> BuildMonthlyTrend(List<Transaction> transactions)
    {
        return transactions
            .GroupBy(t => t.Date.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => new MonthlyMetrics
                {
                    Income = g.Where(t => t.IsIncome).Sum(t => t.Amount),
                    Expense = g.Where(t => t.IsExpense).Sum(t => t.Amount)
                });
    }

    private List<CategoryInsight> BuildCategoryInsights(List<Transaction> transactions, decimal totalExpense)
    {
        return transactions
            .Where(t => t.IsExpense)
            .GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CategoryInsight
            {
                Category = g.Key,
                TotalExpense = g.Sum(x => x.Amount),
                AverageExpense = Math.Round(g.Average(x => x.Amount), 2),
                Count = g.Count(),
                PercentageOfTotalExpense = totalExpense == 0
                    ? 0
                    : Math.Round((g.Sum(x => x.Amount) / totalExpense) * 100m, 2)
            })
            .OrderByDescending(x => x.TotalExpense)
            .ToList();
    }

    private List<MerchantInsight> BuildMerchantInsights(List<Transaction> transactions)
    {
        return transactions
            .Where(t => t.IsExpense)
            .GroupBy(t => t.Merchant, StringComparer.OrdinalIgnoreCase)
            .Select(g => new MerchantInsight
            {
                Merchant = g.Key,
                TotalAmount = g.Sum(x => x.Amount),
                TransactionCount = g.Count(),
                PrimaryCategory = g.GroupBy(x => x.Category)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .FirstOrDefault() ?? "Unknown"
            })
            .OrderByDescending(x => x.TotalAmount)
            .Take(10)
            .ToList();
    }

    private List<string> DetectBudgetOverruns(
        List<CategoryInsight> categoryInsights,
        Dictionary<string, decimal> budgets)
    {
        var alerts = new List<string>();

        foreach (var category in categoryInsights)
        {
            if (!budgets.TryGetValue(category.Category, out var limit))
                continue;

            if (category.TotalExpense > limit)
            {
                var over = category.TotalExpense - limit;
                var usagePct = limit == 0 ? 0 : Math.Round((category.TotalExpense / limit) * 100m, 2);

                alerts.Add(
                    $"[BUDGET ALERT] {category.Category}: Budget={limit:C}, Spent={category.TotalExpense:C}, Over={over:C}, Usage={usagePct:F2}%");
            }
        }

        return alerts;
    }

    private List<string> DetectAnomalies(List<Transaction> transactions)
    {
        var anomalies = new List<string>();

        var expenseGroups = transactions
            .Where(t => t.IsExpense)
            .GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase);

        foreach (var group in expenseGroups)
        {
            var values = group.Select(x => x.Amount).ToList();
            if (values.Count < 3)
                continue;

            var avg = values.Average();
            var stdDev = CalculateStdDev(values);

            foreach (var tx in group)
            {
                bool zScoreLikeOutlier = stdDev > 0 && tx.Amount > avg + (2m * stdDev);
                bool ratioOutlier = avg > 0 && tx.Amount >= avg * 1.75m && tx.Amount >= 1000m;

                if (zScoreLikeOutlier || ratioOutlier)
                {
                    anomalies.Add(
                        $"[ANOMALY] {tx.Date:yyyy-MM-dd} | {tx.Category} | {tx.Merchant} | {tx.Amount:C} | {tx.Description} | Avg={avg:C}");
                }
            }
        }

        return anomalies
            .Distinct()
            .Take(20)
            .ToList();
    }

    private List<string> DetectRecurringPayments(List<Transaction> transactions)
    {
        var recurring = new List<string>();

        var groups = transactions
            .Where(t => t.IsExpense)
            .GroupBy(t => $"{t.Merchant.Trim().ToLowerInvariant()}|{t.Category.Trim().ToLowerInvariant()}")
            .Where(g => g.Count() >= 3);

        foreach (var group in groups)
        {
            var ordered = group.OrderBy(x => x.Date).ToList();
            var intervals = new List<int>();

            for (int i = 1; i < ordered.Count; i++)
            {
                intervals.Add((ordered[i].Date - ordered[i - 1].Date).Days);
            }

            if (intervals.Count == 0)
                continue;

            var avgInterval = intervals.Average();
            var avgAmount = ordered.Average(x => x.Amount);

            bool recurringMonthly = avgInterval >= 25 && avgInterval <= 35;
            bool recurringWeekly = avgInterval >= 6 && avgInterval <= 8;

            if (recurringMonthly || recurringWeekly)
            {
                var sample = ordered.First();
                recurring.Add(
                    $"[RECURRING] Merchant={sample.Merchant}, Category={sample.Category}, AvgAmount={avgAmount:C}, Count={ordered.Count}, AvgInterval={avgInterval:F1} days");
            }
        }

        return recurring;
    }

    private decimal CalculateStdDev(List<decimal> values)
    {
        if (values.Count == 0)
            return 0;

        var avg = values.Average();
        var variance = values.Average(v => (double)((v - avg) * (v - avg)));
        return (decimal)Math.Sqrt(variance);
    }
}