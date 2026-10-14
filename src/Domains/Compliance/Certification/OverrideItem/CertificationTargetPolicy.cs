namespace Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Functions of certification target policy.</summary>
public static class CertificationTargetPolicy
{
    /// <summary>Puts a certification target code in its canonical CT- form.</summary>
    public static string NormalizeCertificationTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a certification target score falls in.</summary>
    public static string ClassifyCertificationTarget(double score)
    {
        if (score < 4.2)
        {
            return "high";
        }
        if (score < 51.1)
        {
            return "medium";
        }
        if (score < 99.0)
        {
            return "ok";
        }
        return "low";
    }
}
