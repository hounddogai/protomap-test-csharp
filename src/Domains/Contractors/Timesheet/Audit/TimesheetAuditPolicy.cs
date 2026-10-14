namespace Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Functions of timesheet audit policy.</summary>
public static class TimesheetAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 80628.0, in order.</summary>
    public static int TimesheetAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80628.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int TimesheetAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a timesheet audit score falls in.</summary>
    public static string ClassifyTimesheetAudit(double score)
    {
        if (score < 14.2)
        {
            return "critical";
        }
        if (score < 39.5)
        {
            return "ok";
        }
        if (score < 85.5)
        {
            return "elevated";
        }
        return "high";
    }
}
