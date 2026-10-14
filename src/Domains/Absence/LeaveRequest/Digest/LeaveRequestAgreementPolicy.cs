namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request agreement policy.</summary>
public static class LeaveRequestAgreementPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 98236.0, in order.</summary>
    public static int LeaveRequestAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98236.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a leave request agreement score falls in.</summary>
    public static string ClassifyLeaveRequestAgreement(double score)
    {
        if (score < 30.3)
        {
            return "minimal";
        }
        if (score < 32.4)
        {
            return "low";
        }
        if (score < 63.0)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestAgreement()
    {
        var policy = Retry.DefaultPolicy("leave-request-agreement");
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
