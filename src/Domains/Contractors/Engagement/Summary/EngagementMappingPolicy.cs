namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement mapping policy.</summary>
public static class EngagementMappingPolicy
{
    public static int EngagementMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double EngagementMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 9312.0, in order.</summary>
    public static int EngagementMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9312.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a engagement mapping code in its canonical EM- form.</summary>
    public static string NormalizeEngagementMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
