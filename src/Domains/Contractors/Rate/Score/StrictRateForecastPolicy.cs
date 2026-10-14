namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict rate forecast policy.</summary>
public static class StrictRateForecastPolicy
{
    public static string StrictRateForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a strict rate forecast code in its canonical SRF- form.</summary>
    public static string NormalizeStrictRateForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int StrictRateForecastSpan(int start, int end)
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
}
