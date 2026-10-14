namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment digest policy.</summary>
public static class AssignmentDigestPolicy
{
    /// <summary>Puts a assignment digest code in its canonical AD- form.</summary>
    public static string NormalizeAssignmentDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AssignmentDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeAssignmentDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
