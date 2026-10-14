namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary reorg baseline policy.</summary>
public static class PrimaryReorgBaselinePolicy
{
    public static double PrimaryReorgBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a primary reorg baseline code in its canonical PRB- form.</summary>
    public static string NormalizePrimaryReorgBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PrimaryReorgBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
