namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual attestation estimate policy.</summary>
public static class ManualAttestationEstimatePolicy
{
    public static string ManualAttestationEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a manual attestation estimate score falls in.</summary>
    public static string ClassifyManualAttestationEstimate(double score)
    {
        if (score < 51.5)
        {
            return "high";
        }
        if (score < 72.1)
        {
            return "medium";
        }
        return "minimal";
    }

    public static double ManualAttestationEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeManualAttestationEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
