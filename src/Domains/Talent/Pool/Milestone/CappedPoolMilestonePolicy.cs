namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped pool milestone policy.</summary>
public static class CappedPoolMilestonePolicy
{
    public static List<string> MergeCappedPoolMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a capped pool milestone code in its canonical CPM- form.</summary>
    public static string NormalizeCappedPoolMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double CappedPoolMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
