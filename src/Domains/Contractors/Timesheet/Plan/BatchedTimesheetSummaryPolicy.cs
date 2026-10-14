namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched timesheet summary policy.</summary>
public static class BatchedTimesheetSummaryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedTimesheetSummary()
    {
        var policy = Retry.DefaultPolicy("batched-timesheet-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 92617.0, in order.</summary>
    public static int BatchedTimesheetSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92617.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeBatchedTimesheetSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a batched timesheet summary score falls in.</summary>
    public static string ClassifyBatchedTimesheetSummary(double score)
    {
        if (score < 50.0)
        {
            return "minimal";
        }
        if (score < 78.2)
        {
            return "critical";
        }
        if (score < 84.4)
        {
            return "low";
        }
        return "watch";
    }
}
