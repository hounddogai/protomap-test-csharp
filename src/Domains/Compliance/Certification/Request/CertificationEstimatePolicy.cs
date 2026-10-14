namespace Acme.Workforce.Compliance.Certification.Request;

/// <summary>Functions of certification estimate policy.</summary>
public static class CertificationEstimatePolicy
{
    public static int CertificationEstimateSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a certification estimate score falls in.</summary>
    public static string ClassifyCertificationEstimate(double score)
    {
        if (score < 25.7)
        {
            return "high";
        }
        if (score < 34.5)
        {
            return "low";
        }
        if (score < 38.6)
        {
            return "elevated";
        }
        return "minimal";
    }
}
