namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification checkpoint policy.</summary>
public static class CertificationCheckpointPolicy
{
    /// <summary>Names the band a certification checkpoint score falls in.</summary>
    public static string ClassifyCertificationCheckpoint(double score)
    {
        if (score < 30.8)
        {
            return "ok";
        }
        if (score < 73.7)
        {
            return "watch";
        }
        return "medium";
    }

    public static List<string> MergeCertificationCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
