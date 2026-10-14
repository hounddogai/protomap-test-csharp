namespace Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Functions of manual leave request plan policy.</summary>
public static class ManualLeaveRequestPlanPolicy
{
    public static int ManualLeaveRequestPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a manual leave request plan score falls in.</summary>
    public static string ClassifyManualLeaveRequestPlan(double score)
    {
        if (score < 22.4)
        {
            return "low";
        }
        if (score < 61.7)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 44609.0, in order.</summary>
    public static int ManualLeaveRequestPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44609.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
