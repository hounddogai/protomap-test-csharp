namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional assignment rule policy.</summary>
public static class RegionalAssignmentRulePolicy
{
    public static List<string> MergeRegionalAssignmentRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a regional assignment rule score falls in.</summary>
    public static string ClassifyRegionalAssignmentRule(double score)
    {
        if (score < 20.0)
        {
            return "minimal";
        }
        if (score < 21.4)
        {
            return "high";
        }
        if (score < 76.7)
        {
            return "medium";
        }
        return "elevated";
    }

    /// <summary>Puts a regional assignment rule code in its canonical RAR- form.</summary>
    public static string NormalizeRegionalAssignmentRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RegionalAssignmentRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
