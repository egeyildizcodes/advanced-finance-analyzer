using System.Text;
using System.Text.Json;
using FinanceAnalyzerPro.Models;
using FinanceAnalyzerPro.Services;

namespace FinanceAnalyzerPro;

internal class Program
{
    private static readonly string DataPath = Path.Combine(AppContext.BaseDirectory, "Data", "transactions.csv");
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "budgetConfig.json");
    private static readonly string ReportsDir = Path.Combine(AppContext.BaseDirectory, "Reports");

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        EnsureEnvironment();

        while (true)
        {
            PrintHeader();

            Console.WriteLine("1) Full analysis");
            Console.WriteLine("2) Show raw transactions");
            Console.WriteLine("3) Generate reports");
            Console.WriteLine("4) Exit");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    RunFullAnalysis(showTransactions: false);
                    break;
                case "2":
                    RunFullAnalysis(showTransactions: true);
                    break;
                case "3":
                    GenerateReportsOnly();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("\nInvalid option.");
                    Pause();
                    break;
            }
        }
    }

    private static void RunFullAnalysis(bool showTransactions)
    {
        try
        {
            var transactions = CsvLoader.LoadTransactions(DataPath);
            var budgetConfig = LoadBudgetConfig(ConfigPath);

            if (showTransactions)
            {
                PrintTransactions(transactions);
            }

            var analyzer = new FinanceAnalyzer();
            var result = analyzer.Analyze(transactions, budgetConfig.CategoryBudgets);

            PrintAnalysis(result);

            ReportWriter.WriteTextReport(result, ReportsDir);
            ReportWriter.WriteJsonReport(result, ReportsDir);

            Console.WriteLine($"\nReports saved to: {ReportsDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
        }

        Pause();
    }

    private static void GenerateReportsOnly()
    {
        try
        {
            var transactions = CsvLoader.LoadTransactions(DataPath);
            var budgetConfig = LoadBudgetConfig(ConfigPath);

            var analyzer = new FinanceAnalyzer();
            var result = analyzer.Analyze(transactions, budgetConfig.CategoryBudgets);

            ReportWriter.WriteTextReport(result, ReportsDir);
            ReportWriter.WriteJsonReport(result, ReportsDir);

            Console.WriteLine($"\nReports generated successfully in: {ReportsDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
        }

        Pause();
    }

    private static BudgetConfig LoadBudgetConfig(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Budget config not found: {path}");
        }

        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<BudgetConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config is null)
        {
            throw new InvalidOperationException("Budget config could not be parsed.");
        }

        return config;
    }

    private static void PrintTransactions(List<Transaction> transactions)
    {
        Console.WriteLine("\n================ RAW TRANSACTIONS ================");
        foreach (var t in transactions.OrderBy(t => t.Date))
        {
            Console.WriteLine(
                $"{t.Date:yyyy-MM-dd} | {t.Type,-7} | {t.Category,-14} | {t.Merchant,-18} | {t.Amount,10:C} | {t.Description}");
        }
        Console.WriteLine("==================================================\n");
    }

    private static void PrintAnalysis(AnalysisResult result)
    {
        Console.WriteLine("\n================ SUMMARY ==========================");
        Console.WriteLine($"Period                : {result.MinDate:yyyy-MM-dd} -> {result.MaxDate:yyyy-MM-dd}");
        Console.WriteLine($"Transaction Count     : {result.TransactionCount}");
        Console.WriteLine($"Total Income          : {result.TotalIncome:C}");
        Console.WriteLine($"Total Expense         : {result.TotalExpense:C}");
        Console.WriteLine($"Net Cash Flow         : {result.NetSavings:C}");
        Console.WriteLine($"Savings Rate          : {result.SavingsRate:F2}%");
        Console.WriteLine($"Avg Monthly Income    : {result.AverageMonthlyIncome:C}");
        Console.WriteLine($"Avg Monthly Expense   : {result.AverageMonthlyExpense:C}");
        Console.WriteLine($"Forecast Next Expense : {result.ForecastNextMonthExpense:C}");
        Console.WriteLine("===================================================\n");

        Console.WriteLine("=============== HEALTH SCORES =====================");
        Console.WriteLine($"Cashflow Score        : {result.FinancialHealth.CashflowScore}/100");
        Console.WriteLine($"Savings Score         : {result.FinancialHealth.SavingsScore}/100");
        Console.WriteLine($"Budget Discipline     : {result.FinancialHealth.BudgetDisciplineScore}/100");
        Console.WriteLine($"Risk Score            : {result.FinancialHealth.RiskScore}/100");
        Console.WriteLine($"Overall Score         : {result.FinancialHealth.OverallScore}/100");
        Console.WriteLine($"Status                : {result.FinancialHealth.Status}");
        Console.WriteLine("===================================================\n");

        Console.WriteLine("=============== CATEGORY INSIGHTS =================");
        foreach (var c in result.CategoryInsights.OrderByDescending(x => x.TotalExpense))
        {
            Console.WriteLine(
                $"{c.Category,-14} | Total: {c.TotalExpense,10:C} | Avg: {c.AverageExpense,10:C} | Count: {c.Count,3} | Share: {c.PercentageOfTotalExpense,6:F2}%");
        }
        Console.WriteLine("===================================================\n");

        Console.WriteLine("================ MONTHLY TREND ====================");
        foreach (var month in result.MonthlyTrend.OrderBy(x => x.Key))
        {
            Console.WriteLine(
                $"{month.Key} | Income: {month.Value.Income,10:C} | Expense: {month.Value.Expense,10:C} | Net: {month.Value.Net,10:C}");
        }
        Console.WriteLine("===================================================\n");

        Console.WriteLine("=============== MERCHANT INSIGHTS =================");
        foreach (var m in result.TopMerchants)
        {
            Console.WriteLine(
                $"{m.Merchant,-20} | Total: {m.TotalAmount,10:C} | Count: {m.TransactionCount,3} | Category: {m.PrimaryCategory}");
        }
        Console.WriteLine("===================================================\n");

        Console.WriteLine("================ BUDGET ALERTS ====================");
        if (result.BudgetAlerts.Count == 0)
        {
            Console.WriteLine("No budget overruns detected.");
        }
        else
        {
            foreach (var alert in result.BudgetAlerts)
            {
                Console.WriteLine(alert);
            }
        }
        Console.WriteLine("===================================================\n");

        Console.WriteLine("================= ANOMALIES =======================");
        if (result.Anomalies.Count == 0)
        {
            Console.WriteLine("No anomalies detected.");
        }
        else
        {
            foreach (var anomaly in result.Anomalies)
            {
                Console.WriteLine(anomaly);
            }
        }
        Console.WriteLine("===================================================\n");

        Console.WriteLine("============= RECURRING PAYMENTS ==================");
        if (result.RecurringPayments.Count == 0)
        {
            Console.WriteLine("No recurring payment patterns detected.");
        }
        else
        {
            foreach (var recurring in result.RecurringPayments)
            {
                Console.WriteLine(recurring);
            }
        }
        Console.WriteLine("===================================================\n");

        Console.WriteLine("============== TOP EXPENSES =======================");
        foreach (var tx in result.TopExpenses)
        {
            Console.WriteLine(
                $"{tx.Date:yyyy-MM-dd} | {tx.Category,-14} | {tx.Merchant,-18} | {tx.Amount,10:C} | {tx.Description}");
        }
        Console.WriteLine("===================================================\n");

        Console.WriteLine("================ RECOMMENDATIONS ==================");
        foreach (var recommendation in result.Recommendations)
        {
            Console.WriteLine($"- {recommendation}");
        }
        Console.WriteLine("===================================================");
    }

    private static void EnsureEnvironment()
    {
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Config"));
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Reports"));
    }

    private static void PrintHeader()
    {
        Console.Clear();
        Console.WriteLine("===================================================");
        Console.WriteLine("          ADVANCED FINANCE ANALYZER PRO");
        Console.WriteLine("===================================================");
        Console.WriteLine($"Transactions : {DataPath}");
        Console.WriteLine($"Budget Config: {ConfigPath}");
        Console.WriteLine($"Reports      : {ReportsDir}");
        Console.WriteLine();
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}