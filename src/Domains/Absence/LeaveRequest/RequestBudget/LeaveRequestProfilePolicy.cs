namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Functions of leave request profile policy.</summary>
public static class LeaveRequestProfilePolicy
{
    /// <summary>Names the band a leave request profile score falls in.</summary>
    public static string ClassifyLeaveRequestProfile(double score)
    {
        if (score < 8.5)
        {
            return "minimal";
        }
        if (score < 44.7)
        {
            return "high";
        }
        if (score < 86.7)
        {
            return "medium";
        }
        if (score < 90.5)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 57894.0, in order.</summary>
    public static int LeaveRequestProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57894.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
