namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched return plan ledger policy.</summary>
public static class BatchedReturnPlanLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 66824.0, in order.</summary>
    public static int BatchedReturnPlanLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66824.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched return plan ledger score falls in.</summary>
    public static string ClassifyBatchedReturnPlanLedger(double score)
    {
        if (score < 57.5)
        {
            return "elevated";
        }
        if (score < 73.2)
        {
            return "watch";
        }
        if (score < 98.9)
        {
            return "ok";
        }
        return "critical";
    }

    public static string BatchedReturnPlanLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
