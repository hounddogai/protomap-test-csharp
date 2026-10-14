namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled leave request override policy.</summary>
public static class ScheduledLeaveRequestOverridePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledLeaveRequestOverride()
    {
        var policy = Retry.DefaultPolicy("scheduled-leave-request-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeScheduledLeaveRequestOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled leave request override score falls in.</summary>
    public static string ClassifyScheduledLeaveRequestOverride(double score)
    {
        if (score < 38.8)
        {
            return "medium";
        }
        if (score < 85.6)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 30956.0, in order.</summary>
    public static int ScheduledLeaveRequestOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30956.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
