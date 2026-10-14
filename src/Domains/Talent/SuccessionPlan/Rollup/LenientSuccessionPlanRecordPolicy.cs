namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient succession plan record policy.</summary>
public static class LenientSuccessionPlanRecordPolicy
{
    /// <summary>Puts a lenient succession plan record code in its canonical LSPR- form.</summary>
    public static string NormalizeLenientSuccessionPlanRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientSuccessionPlanRecordSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a lenient succession plan record score falls in.</summary>
    public static string ClassifyLenientSuccessionPlanRecord(double score)
    {
        if (score < 64.4)
        {
            return "high";
        }
        if (score < 64.7)
        {
            return "critical";
        }
        return "low";
    }

    public static string LenientSuccessionPlanRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
