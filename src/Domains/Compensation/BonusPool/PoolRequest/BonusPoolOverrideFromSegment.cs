namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Functions of bonus pool override from segment.</summary>
public static class BonusPoolOverrideFromSegment
{
    public static Dictionary<string, double> SummarizeLegacyEquityGrantSegmentsForOverride(List<LegacyEquityGrantSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<BonusPoolOverride> DeriveBonusPoolOverridesFromSegment(List<LegacyEquityGrantSegment> items)
    {
        var outValue = new List<BonusPoolOverride>();
        foreach (var item in items)
        {
            var derived = BonusPoolOverride.NewBonusPoolOverride(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignBonusPoolOverride(BonusPoolOverride item)
    {
        return EquityGrantMilestonePolicy.EquityGrantMilestoneWeight(8, true);
    }
}
