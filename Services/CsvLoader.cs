using System.Globalization;
using FinanceAnalyzerPro.Models;

namespace FinanceAnalyzerPro.Services;

public static class CsvLoader
{
    public static List<Transaction> LoadTransactions(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Transaction file not found: {path}");
        }

        var lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            throw new InvalidOperationException("CSV file is empty or has no data rows.");
        }

        var transactions = new List<Transaction>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = SplitCsv(line);
            if (parts.Count < 6)
            {
                throw new FormatException($"Invalid CSV format at line {i + 1}: {line}");
            }

            if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                throw new FormatException($"Invalid date at line {i + 1}: {parts[0]}");
            }

            if (!decimal.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                throw new FormatException($"Invalid amount at line {i + 1}: {parts[4]}");
            }

            var type = parts[5].Trim();
            if (!type.Equals("Income", StringComparison.OrdinalIgnoreCase) &&
                !type.Equals("Expense", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException($"Type must be Income or Expense at line {i + 1}: {type}");
            }

            transactions.Add(new Transaction
            {
                Date = date,
                Description = parts[1].Trim(),
                Category = parts[2].Trim(),
                Merchant = parts[3].Trim(),
                Amount = Math.Abs(amount),
                Type = type
            });
        }

        return transactions;
    }

    private static List<string> SplitCsv(string line)
    {
        var result = new List<string>();
        var current = "";
        bool inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result;
    }
}