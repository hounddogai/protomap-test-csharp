namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of timesheet exception policy.</summary>
public static class TimesheetExceptionPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTimesheetException()
    {
        var policy = Retry.DefaultPolicy("timesheet-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeTimesheetExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 88632.0, in order.</summary>
    public static int TimesheetExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88632.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a timesheet exception score falls in.</summary>
    public static string ClassifyTimesheetException(double score)
    {
        if (score < 47.7)
        {
            return "minimal";
        }
        if (score < 51.3)
        {
            return "elevated";
        }
        if (score < 64.2)
        {
            return "medium";
        }
        return "critical";
    }
}
