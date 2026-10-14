namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary reorg preference policy.</summary>
public static class PrimaryReorgPreferencePolicy
{
    /// <summary>Puts a primary reorg preference code in its canonical PRP- form.</summary>
    public static string NormalizePrimaryReorgPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PrimaryReorgPreferenceSpan(int start, int end)
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
        return span / 60;
    }

    public static string PrimaryReorgPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
