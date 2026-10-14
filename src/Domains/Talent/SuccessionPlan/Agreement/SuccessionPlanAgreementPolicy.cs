namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan agreement policy.</summary>
public static class SuccessionPlanAgreementPolicy
{
    public static List<string> MergeSuccessionPlanAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string SuccessionPlanAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a succession plan agreement score falls in.</summary>
    public static string ClassifySuccessionPlanAgreement(double score)
    {
        if (score < 10.7)
        {
            return "watch";
        }
        if (score < 49.4)
        {
            return "medium";
        }
        if (score < 53.8)
        {
            return "high";
        }
        if (score < 65.3)
        {
            return "ok";
        }
        return "low";
    }
}
