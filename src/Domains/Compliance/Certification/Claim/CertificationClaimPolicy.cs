namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification claim policy.</summary>
public static class CertificationClaimPolicy
{
    /// <summary>Names the band a certification claim score falls in.</summary>
    public static string ClassifyCertificationClaim(double score)
    {
        if (score < 28.8)
        {
            return "low";
        }
        if (score < 49.3)
        {
            return "elevated";
        }
        return "critical";
    }

    public static int CertificationClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a certification claim code in its canonical CC- form.</summary>
    public static string NormalizeCertificationClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CertificationClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
