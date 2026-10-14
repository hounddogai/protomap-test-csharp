namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled access badge metric policy.</summary>
public static class ScheduledAccessBadgeMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 84513.0, in order.</summary>
    public static int ScheduledAccessBadgeMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84513.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeScheduledAccessBadgeMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ScheduledAccessBadgeMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a scheduled access badge metric code in its canonical SABM- form.</summary>
    public static string NormalizeScheduledAccessBadgeMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SABM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SABM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
