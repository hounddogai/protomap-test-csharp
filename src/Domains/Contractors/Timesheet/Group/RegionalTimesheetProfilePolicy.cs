namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional timesheet profile policy.</summary>
public static class RegionalTimesheetProfilePolicy
{
    /// <summary>Names the band a regional timesheet profile score falls in.</summary>
    public static string ClassifyRegionalTimesheetProfile(double score)
    {
        if (score < 23.5)
        {
            return "elevated";
        }
        if (score < 43.6)
        {
            return "low";
        }
        if (score < 88.7)
        {
            return "high";
        }
        return "minimal";
    }

    public static int RegionalTimesheetProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalTimesheetProfile()
    {
        var policy = Retry.DefaultPolicy("regional-timesheet-profile");
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

    public static double RegionalTimesheetProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
