namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Functions of fallback training due rollup from threshold.</summary>
public static class FallbackTrainingDueRollupFromThreshold
{
    public static Dictionary<string, double> SummarizePendingAuditThresholdsForRollup(List<PendingAuditThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<FallbackTrainingDueRollup> DeriveFallbackTrainingDueRollupsFromThreshold(List<PendingAuditThreshold> items)
    {
        var outValue = new List<FallbackTrainingDueRollup>();
        foreach (var item in items)
        {
            var derived = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceFallbackTrainingDueRollup(FallbackTrainingDueRollup item)
    {
        return WeightedAuditRecordPolicy.NormalizeWeightedAuditRecordCode(item.Code);
    }
}
