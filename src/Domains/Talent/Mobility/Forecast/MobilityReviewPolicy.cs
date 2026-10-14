namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility review policy.</summary>
public static class MobilityReviewPolicy
{
    public static string MobilityReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a mobility review code in its canonical MR- form.</summary>
    public static string NormalizeMobilityReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int MobilityReviewSpan(int start, int end)
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
