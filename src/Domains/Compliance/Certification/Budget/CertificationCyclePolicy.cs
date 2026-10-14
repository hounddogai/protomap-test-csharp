namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification cycle policy.</summary>
public static class CertificationCyclePolicy
{
    public static string CertificationCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a certification cycle score falls in.</summary>
    public static string ClassifyCertificationCycle(double score)
    {
        if (score < 2.0)
        {
            return "high";
        }
        if (score < 39.0)
        {
            return "watch";
        }
        if (score < 89.9)
        {
            return "ok";
        }
        if (score < 92.3)
        {
            return "minimal";
        }
        return "medium";
    }

    public static List<string> MergeCertificationCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
