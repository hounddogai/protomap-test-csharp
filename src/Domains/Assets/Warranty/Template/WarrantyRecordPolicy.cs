namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty record policy.</summary>
public static class WarrantyRecordPolicy
{
    public static string WarrantyRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a warranty record code in its canonical WR- form.</summary>
    public static string NormalizeWarrantyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int WarrantyRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 46425.0, in order.</summary>
    public static int WarrantyRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46425.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
