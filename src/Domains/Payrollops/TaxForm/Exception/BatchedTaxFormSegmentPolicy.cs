namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched tax form segment policy.</summary>
public static class BatchedTaxFormSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedTaxFormSegment()
    {
        var policy = Retry.DefaultPolicy("batched-tax-form-segment");
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

    /// <summary>Counts how many amounts fit in a budget of 52163.0, in order.</summary>
    public static int BatchedTaxFormSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52163.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
