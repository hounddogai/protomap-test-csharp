namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled engagement group policy.</summary>
public static class ScheduledEngagementGroupPolicy
{
    public static int ScheduledEngagementGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static List<string> MergeScheduledEngagementGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double ScheduledEngagementGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a scheduled engagement group code in its canonical SEG- form.</summary>
    public static string NormalizeScheduledEngagementGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SEG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SEG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
