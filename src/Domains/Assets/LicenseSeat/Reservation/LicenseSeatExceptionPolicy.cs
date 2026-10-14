namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat exception policy.</summary>
public static class LicenseSeatExceptionPolicy
{
    public static double LicenseSeatExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string LicenseSeatExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a license seat exception code in its canonical LSE- form.</summary>
    public static string NormalizeLicenseSeatExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a license seat exception score falls in.</summary>
    public static string ClassifyLicenseSeatException(double score)
    {
        if (score < 18.8)
        {
            return "medium";
        }
        if (score < 60.5)
        {
            return "critical";
        }
        if (score < 98.2)
        {
            return "low";
        }
        return "watch";
    }
}
