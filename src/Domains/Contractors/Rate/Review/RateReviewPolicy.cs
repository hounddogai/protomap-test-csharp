namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate review policy.</summary>
public static class RateReviewPolicy
{
    public static string RateReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int RateReviewSpan(int start, int end)
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

    /// <summary>Puts a rate review code in its canonical RR- form.</summary>
    public static string NormalizeRateReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
