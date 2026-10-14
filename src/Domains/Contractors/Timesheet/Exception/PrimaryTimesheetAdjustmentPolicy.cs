namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary timesheet adjustment policy.</summary>
public static class PrimaryTimesheetAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 38081.0, in order.</summary>
    public static int PrimaryTimesheetAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38081.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a primary timesheet adjustment score falls in.</summary>
    public static string ClassifyPrimaryTimesheetAdjustment(double score)
    {
        if (score < 11.8)
        {
            return "high";
        }
        if (score < 13.6)
        {
            return "medium";
        }
        return "ok";
    }

    public static List<string> MergePrimaryTimesheetAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryTimesheetAdjustment()
    {
        var policy = Retry.DefaultPolicy("primary-timesheet-adjustment");
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
}
