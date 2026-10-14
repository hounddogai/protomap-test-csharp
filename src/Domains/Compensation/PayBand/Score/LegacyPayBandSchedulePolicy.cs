namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy pay band schedule policy.</summary>
public static class LegacyPayBandSchedulePolicy
{
    /// <summary>Puts a legacy pay band schedule code in its canonical LPBS- form.</summary>
    public static string NormalizeLegacyPayBandScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPBS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPBS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy pay band schedule score falls in.</summary>
    public static string ClassifyLegacyPayBandSchedule(double score)
    {
        if (score < 8.9)
        {
            return "low";
        }
        if (score < 84.9)
        {
            return "medium";
        }
        return "watch";
    }

    public static int LegacyPayBandScheduleSpan(int start, int end)
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

    public static double LegacyPayBandScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
