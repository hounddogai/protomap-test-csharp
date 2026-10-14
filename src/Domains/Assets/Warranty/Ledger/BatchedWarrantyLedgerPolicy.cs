namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched warranty ledger policy.</summary>
public static class BatchedWarrantyLedgerPolicy
{
    /// <summary>Names the band a batched warranty ledger score falls in.</summary>
    public static string ClassifyBatchedWarrantyLedger(double score)
    {
        if (score < 72.4)
        {
            return "elevated";
        }
        if (score < 74.0)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 86867.0, in order.</summary>
    public static int BatchedWarrantyLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86867.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedWarrantyLedger()
    {
        var policy = Retry.DefaultPolicy("batched-warranty-ledger");
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
}
