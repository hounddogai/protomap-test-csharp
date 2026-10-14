namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay run assignment policy.</summary>
public static class PayRunAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 93413.0, in order.</summary>
    public static int PayRunAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93413.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergePayRunAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a pay run assignment score falls in.</summary>
    public static string ClassifyPayRunAssignment(double score)
    {
        if (score < 8.1)
        {
            return "low";
        }
        if (score < 99.4)
        {
            return "minimal";
        }
        return "watch";
    }
}
