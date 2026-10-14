namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of return plan claim policy.</summary>
public static class ReturnPlanClaimPolicy
{
    /// <summary>Names the band a return plan claim score falls in.</summary>
    public static string ClassifyReturnPlanClaim(double score)
    {
        if (score < 4.4)
        {
            return "elevated";
        }
        if (score < 41.3)
        {
            return "ok";
        }
        if (score < 60.2)
        {
            return "low";
        }
        return "minimal";
    }

    public static List<string> MergeReturnPlanClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double ReturnPlanClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ReturnPlanClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
