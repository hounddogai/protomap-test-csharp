namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched tax form ledger policy.</summary>
public static class BatchedTaxFormLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedTaxFormLedger()
    {
        var policy = Retry.DefaultPolicy("batched-tax-form-ledger");
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

    public static int BatchedTaxFormLedgerSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a batched tax form ledger score falls in.</summary>
    public static string ClassifyBatchedTaxFormLedger(double score)
    {
        if (score < 20.0)
        {
            return "critical";
        }
        if (score < 47.9)
        {
            return "elevated";
        }
        if (score < 49.0)
        {
            return "ok";
        }
        if (score < 51.6)
        {
            return "high";
        }
        return "medium";
    }
}
