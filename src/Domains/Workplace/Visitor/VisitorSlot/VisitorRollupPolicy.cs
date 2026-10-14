namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor rollup policy.</summary>
public static class VisitorRollupPolicy
{
    /// <summary>Puts a visitor rollup code in its canonical VR- form.</summary>
    public static string NormalizeVisitorRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double VisitorRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeVisitorRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
