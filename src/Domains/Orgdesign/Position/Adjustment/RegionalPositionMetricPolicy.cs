namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional position metric policy.</summary>
public static class RegionalPositionMetricPolicy
{
    public static string RegionalPositionMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 61734.0, in order.</summary>
    public static int RegionalPositionMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61734.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RegionalPositionMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a regional position metric code in its canonical RPM- form.</summary>
    public static string NormalizeRegionalPositionMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
