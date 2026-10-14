namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of attestation estimate policy.</summary>
public static class AttestationEstimatePolicy
{
    /// <summary>Names the band a attestation estimate score falls in.</summary>
    public static string ClassifyAttestationEstimate(double score)
    {
        if (score < 37.0)
        {
            return "ok";
        }
        if (score < 78.5)
        {
            return "high";
        }
        if (score < 86.8)
        {
            return "elevated";
        }
        if (score < 87.2)
        {
            return "critical";
        }
        return "minimal";
    }

    public static List<string> MergeAttestationEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string AttestationEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
