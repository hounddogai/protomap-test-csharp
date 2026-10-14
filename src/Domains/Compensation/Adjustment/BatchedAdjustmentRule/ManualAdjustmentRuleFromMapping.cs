namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Functions of manual adjustment rule from mapping.</summary>
public static class ManualAdjustmentRuleFromMapping
{
    public static Dictionary<string, double> SummarizeTieredAdjustmentMappingsForRule(List<TieredAdjustmentMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<ManualAdjustmentRule> DeriveManualAdjustmentRulesFromMapping(List<TieredAdjustmentMapping> items)
    {
        var outValue = new List<ManualAdjustmentRule>();
        foreach (var item in items)
        {
            var derived = ManualAdjustmentRule.NewManualAdjustmentRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileManualAdjustmentRule(ManualAdjustmentRule item)
    {
        return AdjustmentBatchPolicy.AdjustmentBatchBucket(item.WeightedExposure());
    }
}
