namespace Acme.Workforce.Assets.Assignment.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment rollup policy.</summary>
public static class AssignmentRollupPolicy
{
    public static string AssignmentRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a assignment rollup code in its canonical AR- form.</summary>
    public static string NormalizeAssignmentRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAssignmentRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a assignment rollup score falls in.</summary>
    public static string ClassifyAssignmentRollup(double score)
    {
        if (score < 12.3)
        {
            return "watch";
        }
        if (score < 16.9)
        {
            return "minimal";
        }
        return "ok";
    }
}
