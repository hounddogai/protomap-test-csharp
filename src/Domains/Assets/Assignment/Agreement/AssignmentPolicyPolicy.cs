namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment policy policy.</summary>
public static class AssignmentPolicyPolicy
{
    /// <summary>Puts a assignment policy code in its canonical AP- form.</summary>
    public static string NormalizeAssignmentPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAssignmentPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int AssignmentPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
