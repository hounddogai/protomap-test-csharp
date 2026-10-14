namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient leave request forecast policy.</summary>
public static class LenientLeaveRequestForecastPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientLeaveRequestForecast()
    {
        var policy = Retry.DefaultPolicy("lenient-leave-request-forecast");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a lenient leave request forecast score falls in.</summary>
    public static string ClassifyLenientLeaveRequestForecast(double score)
    {
        if (score < 7.3)
        {
            return "elevated";
        }
        if (score < 8.6)
        {
            return "ok";
        }
        if (score < 24.1)
        {
            return "watch";
        }
        if (score < 42.3)
        {
            return "minimal";
        }
        return "critical";
    }

    public static string LenientLeaveRequestForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int LenientLeaveRequestForecastSpan(int start, int end)
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
        return span / 86400;
    }
}
