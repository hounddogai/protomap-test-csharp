namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request batch policy.</summary>
public static class LeaveRequestBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestBatch()
    {
        var policy = Retry.DefaultPolicy("leave-request-batch");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int LeaveRequestBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a leave request batch score falls in.</summary>
    public static string ClassifyLeaveRequestBatch(double score)
    {
        if (score < 6.6)
        {
            return "high";
        }
        if (score < 31.9)
        {
            return "ok";
        }
        return "medium";
    }
}
