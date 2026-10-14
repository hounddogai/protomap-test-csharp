namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of reorg preference policy.</summary>
public static class ReorgPreferencePolicy
{
    /// <summary>Puts a reorg preference code in its canonical RP- form.</summary>
    public static string NormalizeReorgPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ReorgPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
