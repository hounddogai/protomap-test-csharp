namespace Acme.Workforce.Compliance.Certification.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification manifest policy.</summary>
public static class CertificationManifestPolicy
{
    public static double CertificationManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int CertificationManifestSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a certification manifest score falls in.</summary>
    public static string ClassifyCertificationManifest(double score)
    {
        if (score < 70.4)
        {
            return "watch";
        }
        if (score < 72.6)
        {
            return "high";
        }
        if (score < 80.8)
        {
            return "low";
        }
        if (score < 94.6)
        {
            return "critical";
        }
        return "ok";
    }
}
