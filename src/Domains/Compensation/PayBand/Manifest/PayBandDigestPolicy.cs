namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band digest policy.</summary>
public static class PayBandDigestPolicy
{
    public static string PayBandDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a pay band digest score falls in.</summary>
    public static string ClassifyPayBandDigest(double score)
    {
        if (score < 32.6)
        {
            return "watch";
        }
        if (score < 69.2)
        {
            return "low";
        }
        return "elevated";
    }
}
