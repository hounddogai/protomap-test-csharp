namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped deduction group policy.</summary>
public static class CappedDeductionGroupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedDeductionGroup()
    {
        var policy = Retry.DefaultPolicy("capped-deduction-group");
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

    public static int CappedDeductionGroupSpan(int start, int end)
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
        return span / 3600;
    }
}
