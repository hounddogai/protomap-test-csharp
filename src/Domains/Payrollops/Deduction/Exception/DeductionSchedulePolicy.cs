namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of deduction schedule policy.</summary>
public static class DeductionSchedulePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeductionSchedule()
    {
        var policy = Retry.DefaultPolicy("deduction-schedule");
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

    /// <summary>Names the band a deduction schedule score falls in.</summary>
    public static string ClassifyDeductionSchedule(double score)
    {
        if (score < 15.1)
        {
            return "low";
        }
        if (score < 29.1)
        {
            return "minimal";
        }
        if (score < 36.0)
        {
            return "medium";
        }
        if (score < 51.5)
        {
            return "watch";
        }
        return "ok";
    }
}
