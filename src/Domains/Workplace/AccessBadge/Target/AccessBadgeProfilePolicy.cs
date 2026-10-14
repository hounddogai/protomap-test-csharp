namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of access badge profile policy.</summary>
public static class AccessBadgeProfilePolicy
{
    /// <summary>Puts a access badge profile code in its canonical ABP- form.</summary>
    public static string NormalizeAccessBadgeProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AccessBadgeProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 37980.0, in order.</summary>
    public static int AccessBadgeProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37980.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
