namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling certification summary policy.</summary>
public static class RollingCertificationSummaryPolicy
{
    public static double RollingCertificationSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.88;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a rolling certification summary score falls in.</summary>
    public static string ClassifyRollingCertificationSummary(double score)
    {
        if (score < 55.5)
        {
            return "low";
        }
        if (score < 78.6)
        {
            return "minimal";
        }
        if (score < 91.8)
        {
            return "elevated";
        }
        if (score < 96.2)
        {
            return "watch";
        }
        return "medium";
    }

    public static int RollingCertificationSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeRollingCertificationSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
