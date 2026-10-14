namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment metric policy.</summary>
public static class AssignmentMetricPolicy
{
    public static List<string> MergeAssignmentMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a assignment metric code in its canonical AM- form.</summary>
    public static string NormalizeAssignmentMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
