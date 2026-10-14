namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling reorg target policy.</summary>
public static class RollingReorgTargetPolicy
{
    public static List<string> MergeRollingReorgTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a rolling reorg target code in its canonical RRT- form.</summary>
    public static string NormalizeRollingReorgTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RollingReorgTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
