namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan cycle policy.</summary>
public static class SuccessionPlanCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 83811.0, in order.</summary>
    public static int SuccessionPlanCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83811.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SuccessionPlanCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a succession plan cycle score falls in.</summary>
    public static string ClassifySuccessionPlanCycle(double score)
    {
        if (score < 10.9)
        {
            return "watch";
        }
        if (score < 38.6)
        {
            return "medium";
        }
        if (score < 72.3)
        {
            return "high";
        }
        return "ok";
    }

    public static List<string> MergeSuccessionPlanCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
