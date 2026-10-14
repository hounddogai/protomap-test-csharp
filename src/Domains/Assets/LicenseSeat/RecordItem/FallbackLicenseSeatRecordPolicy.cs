namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback license seat record policy.</summary>
public static class FallbackLicenseSeatRecordPolicy
{
    /// <summary>Puts a fallback license seat record code in its canonical FLSR- form.</summary>
    public static string NormalizeFallbackLicenseSeatRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FLSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FLSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackLicenseSeatRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a fallback license seat record score falls in.</summary>
    public static string ClassifyFallbackLicenseSeatRecord(double score)
    {
        if (score < 6.1)
        {
            return "low";
        }
        if (score < 16.4)
        {
            return "high";
        }
        if (score < 43.5)
        {
            return "ok";
        }
        if (score < 48.0)
        {
            return "watch";
        }
        return "critical";
    }
}
