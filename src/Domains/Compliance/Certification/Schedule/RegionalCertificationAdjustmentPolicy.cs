namespace Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Functions of regional certification adjustment policy.</summary>
public static class RegionalCertificationAdjustmentPolicy
{
    /// <summary>Names the band a regional certification adjustment score falls in.</summary>
    public static string ClassifyRegionalCertificationAdjustment(double score)
    {
        if (score < 27.9)
        {
            return "medium";
        }
        if (score < 62.5)
        {
            return "ok";
        }
        return "watch";
    }

    public static int RegionalCertificationAdjustmentSpan(int start, int end)
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

    /// <summary>Puts a regional certification adjustment code in its canonical RCA- form.</summary>
    public static string NormalizeRegionalCertificationAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
