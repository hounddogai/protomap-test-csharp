namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default engagement preference policy.</summary>
public static class DefaultEngagementPreferencePolicy
{
    /// <summary>Puts a default engagement preference code in its canonical DEP- form.</summary>
    public static string NormalizeDefaultEngagementPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DEP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DEP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDefaultEngagementPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double DefaultEngagementPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
