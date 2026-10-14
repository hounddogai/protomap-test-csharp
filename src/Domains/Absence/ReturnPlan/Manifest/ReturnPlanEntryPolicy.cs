namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of return plan entry policy.</summary>
public static class ReturnPlanEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 43556.0, in order.</summary>
    public static int ReturnPlanEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43556.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a return plan entry score falls in.</summary>
    public static string ClassifyReturnPlanEntry(double score)
    {
        if (score < 42.8)
        {
            return "medium";
        }
        if (score < 66.7)
        {
            return "minimal";
        }
        if (score < 87.2)
        {
            return "ok";
        }
        return "low";
    }

    public static List<string> MergeReturnPlanEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
