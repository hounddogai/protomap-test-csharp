namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of reorg score policy.</summary>
public static class ReorgScorePolicy
{
    public static List<string> MergeReorgScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a reorg score code in its canonical RS- form.</summary>
    public static string NormalizeReorgScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ReorgScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
