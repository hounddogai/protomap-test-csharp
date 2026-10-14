namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>Functions of fallback balance threshold from audit.</summary>
public static class FallbackBalanceThresholdFromAudit
{
    public static Dictionary<string, double> SummarizeReturnPlanAuditsForThreshold(List<ReturnPlanAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<FallbackBalanceThreshold> DeriveFallbackBalanceThresholdsFromAudit(List<ReturnPlanAudit> items)
    {
        var outValue = new List<FallbackBalanceThreshold>();
        foreach (var item in items)
        {
            var derived = FallbackBalanceThreshold.NewFallbackBalanceThreshold(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanFallbackBalanceThreshold(FallbackBalanceThreshold item)
    {
        return ReturnPlanDraftPolicy.ReturnPlanDraftBucket(item.NetLoad());
    }
}
