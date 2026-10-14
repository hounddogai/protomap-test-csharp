namespace Acme.Workforce.Compliance.Attestation.Baseline;

/// <summary>Functions of capped attestation window policy.</summary>
public static class CappedAttestationWindowPolicy
{
    public static int CappedAttestationWindowSpan(int start, int end)
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

    /// <summary>Names the band a capped attestation window score falls in.</summary>
    public static string ClassifyCappedAttestationWindow(double score)
    {
        if (score < 56.3)
        {
            return "high";
        }
        if (score < 58.6)
        {
            return "low";
        }
        if (score < 71.2)
        {
            return "watch";
        }
        return "elevated";
    }
}
