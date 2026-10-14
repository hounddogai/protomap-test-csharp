namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request segment policy.</summary>
public static class LeaveRequestSegmentPolicy
{
    /// <summary>Names the band a leave request segment score falls in.</summary>
    public static string ClassifyLeaveRequestSegment(double score)
    {
        if (score < 43.1)
        {
            return "elevated";
        }
        if (score < 44.7)
        {
            return "watch";
        }
        if (score < 50.6)
        {
            return "medium";
        }
        if (score < 73.8)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 87353.0, in order.</summary>
    public static int LeaveRequestSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87353.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LeaveRequestSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestSegment()
    {
        var policy = Retry.DefaultPolicy("leave-request-segment");
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
}
