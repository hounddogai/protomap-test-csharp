namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual attestation segment policy.</summary>
public static class ManualAttestationSegmentPolicy
{
    public static List<string> MergeManualAttestationSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a manual attestation segment score falls in.</summary>
    public static string ClassifyManualAttestationSegment(double score)
    {
        if (score < 27.6)
        {
            return "watch";
        }
        if (score < 53.1)
        {
            return "low";
        }
        if (score < 96.5)
        {
            return "elevated";
        }
        if (score < 96.6)
        {
            return "ok";
        }
        return "critical";
    }
}
