namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment item policy.</summary>
public static class AssignmentItemPolicy
{
    /// <summary>Puts a assignment item code in its canonical AI- form.</summary>
    public static string NormalizeAssignmentItemCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AI-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AI-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a assignment item score falls in.</summary>
    public static string ClassifyAssignmentItem(double score)
    {
        if (score < 27.8)
        {
            return "low";
        }
        if (score < 89.3)
        {
            return "high";
        }
        return "minimal";
    }

    public static List<string> MergeAssignmentItemLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
