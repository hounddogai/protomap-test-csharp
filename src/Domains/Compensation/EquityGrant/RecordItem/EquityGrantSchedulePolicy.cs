namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant schedule policy.</summary>
public static class EquityGrantSchedulePolicy
{
    /// <summary>Puts a equity grant schedule code in its canonical EGS- form.</summary>
    public static string NormalizeEquityGrantScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int EquityGrantScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 85502.0, in order.</summary>
    public static int EquityGrantScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85502.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string EquityGrantScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
