namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Functions of return plan batch from audit.</summary>
public static class ReturnPlanBatchFromAudit
{
    public static Dictionary<string, double> SummarizeBalanceAuditsForBatch(List<BalanceAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ReturnPlanBatch> DeriveReturnPlanBatchesFromAudit(List<BalanceAudit> items)
    {
        var outValue = new List<ReturnPlanBatch>();
        foreach (var item in items)
        {
            var derived = ReturnPlanBatch.NewReturnPlanBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeReturnPlanBatch(ReturnPlanBatch item)
    {
        return BatchedBalanceRulePolicy.BatchedBalanceRuleBucket(item.MeasuredValue());
    }
}
