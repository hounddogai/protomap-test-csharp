namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet plan policy.</summary>
public static class TimesheetPlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 51002.0, in order.</summary>
    public static int TimesheetPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51002.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a timesheet plan score falls in.</summary>
    public static string ClassifyTimesheetPlan(double score)
    {
        if (score < 16.5)
        {
            return "ok";
        }
        if (score < 35.4)
        {
            return "high";
        }
        if (score < 67.1)
        {
            return "critical";
        }
        return "watch";
    }

    public static List<string> MergeTimesheetPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
