namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional succession plan summary policy.</summary>
public static class RegionalSuccessionPlanSummaryPolicy
{
    /// <summary>Names the band a regional succession plan summary score falls in.</summary>
    public static string ClassifyRegionalSuccessionPlanSummary(double score)
    {
        if (score < 55.8)
        {
            return "medium";
        }
        if (score < 65.6)
        {
            return "watch";
        }
        if (score < 67.7)
        {
            return "ok";
        }
        if (score < 85.1)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 35209.0, in order.</summary>
    public static int RegionalSuccessionPlanSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35209.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeRegionalSuccessionPlanSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RegionalSuccessionPlanSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
