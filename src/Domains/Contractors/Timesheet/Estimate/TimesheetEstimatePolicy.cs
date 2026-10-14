namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of timesheet estimate policy.</summary>
public static class TimesheetEstimatePolicy
{
    public static string TimesheetEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double TimesheetEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTimesheetEstimate()
    {
        var policy = Retry.DefaultPolicy("timesheet-estimate");
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

    /// <summary>Names the band a timesheet estimate score falls in.</summary>
    public static string ClassifyTimesheetEstimate(double score)
    {
        if (score < 16.8)
        {
            return "medium";
        }
        if (score < 98.4)
        {
            return "critical";
        }
        return "low";
    }
}
