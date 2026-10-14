namespace Acme.Workforce.Assets.Assignment.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional assignment entry policy.</summary>
public static class RegionalAssignmentEntryPolicy
{
    public static double RegionalAssignmentEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeRegionalAssignmentEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a regional assignment entry code in its canonical RAE- form.</summary>
    public static string NormalizeRegionalAssignmentEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
