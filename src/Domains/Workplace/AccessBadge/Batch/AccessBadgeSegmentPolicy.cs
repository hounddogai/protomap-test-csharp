namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of access badge segment policy.</summary>
public static class AccessBadgeSegmentPolicy
{
    public static string AccessBadgeSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 96248.0, in order.</summary>
    public static int AccessBadgeSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96248.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a access badge segment code in its canonical ABS- form.</summary>
    public static string NormalizeAccessBadgeSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
