namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of return plan summary policy.</summary>
public static class ReturnPlanSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 90865.0, in order.</summary>
    public static int ReturnPlanSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90865.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeReturnPlanSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
