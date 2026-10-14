namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request exception policy.</summary>
public static class LeaveRequestExceptionPolicy
{
    /// <summary>Names the band a leave request exception score falls in.</summary>
    public static string ClassifyLeaveRequestException(double score)
    {
        if (score < 24.9)
        {
            return "high";
        }
        if (score < 33.1)
        {
            return "medium";
        }
        return "low";
    }

    public static int LeaveRequestExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestException()
    {
        var policy = Retry.DefaultPolicy("leave-request-exception");
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
