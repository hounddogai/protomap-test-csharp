namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Functions of adjustment threshold from slot.</summary>
public static class AdjustmentThresholdFromSlot
{
    public static Dictionary<string, double> SummarizeRollingBonusPoolSlotsForThreshold(List<RollingBonusPoolSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<AdjustmentThreshold> DeriveAdjustmentThresholdsFromSlot(List<RollingBonusPoolSlot> items)
    {
        var outValue = new List<AdjustmentThreshold>();
        foreach (var item in items)
        {
            var derived = AdjustmentThreshold.NewAdjustmentThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreAdjustmentThreshold(AdjustmentThreshold item)
    {
        return LegacyBonusPoolAllocationPolicy.LegacyBonusPoolAllocationBucket(item.MeasuredValue());
    }
}
