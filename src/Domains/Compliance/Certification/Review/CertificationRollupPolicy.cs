namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification rollup policy.</summary>
public static class CertificationRollupPolicy
{
    public static List<string> MergeCertificationRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a certification rollup score falls in.</summary>
    public static string ClassifyCertificationRollup(double score)
    {
        if (score < 12.2)
        {
            return "low";
        }
        if (score < 65.9)
        {
            return "high";
        }
        return "critical";
    }

    public static int CertificationRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
