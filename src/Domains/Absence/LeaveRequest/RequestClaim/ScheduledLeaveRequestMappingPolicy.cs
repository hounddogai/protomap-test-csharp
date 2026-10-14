namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled leave request mapping policy.</summary>
public static class ScheduledLeaveRequestMappingPolicy
{
    /// <summary>Names the band a scheduled leave request mapping score falls in.</summary>
    public static string ClassifyScheduledLeaveRequestMapping(double score)
    {
        if (score < 6.5)
        {
            return "watch";
        }
        if (score < 18.0)
        {
            return "high";
        }
        if (score < 94.9)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledLeaveRequestMapping()
    {
        var policy = Retry.DefaultPolicy("scheduled-leave-request-mapping");
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

    public static double ScheduledLeaveRequestMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledLeaveRequestMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
