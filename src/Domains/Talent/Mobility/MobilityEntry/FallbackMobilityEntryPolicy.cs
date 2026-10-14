namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback mobility entry policy.</summary>
public static class FallbackMobilityEntryPolicy
{
    /// <summary>Names the band a fallback mobility entry score falls in.</summary>
    public static string ClassifyFallbackMobilityEntry(double score)
    {
        if (score < 83.6)
        {
            return "medium";
        }
        if (score < 96.4)
        {
            return "elevated";
        }
        return "critical";
    }

    public static string FallbackMobilityEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a fallback mobility entry code in its canonical FME- form.</summary>
    public static string NormalizeFallbackMobilityEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FME-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FME-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int FallbackMobilityEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
