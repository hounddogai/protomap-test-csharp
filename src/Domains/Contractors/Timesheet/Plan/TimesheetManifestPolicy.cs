namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of timesheet manifest policy.</summary>
public static class TimesheetManifestPolicy
{
    public static double TimesheetManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 81140.0, in order.</summary>
    public static int TimesheetManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81140.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTimesheetManifest()
    {
        var policy = Retry.DefaultPolicy("timesheet-manifest");
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

    /// <summary>Names the band a timesheet manifest score falls in.</summary>
    public static string ClassifyTimesheetManifest(double score)
    {
        if (score < 2.5)
        {
            return "medium";
        }
        if (score < 2.9)
        {
            return "high";
        }
        if (score < 9.7)
        {
            return "watch";
        }
        if (score < 50.0)
        {
            return "critical";
        }
        return "low";
    }
}
