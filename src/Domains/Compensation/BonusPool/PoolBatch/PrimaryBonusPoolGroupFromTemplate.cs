namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Functions of primary bonus pool group from template.</summary>
public static class PrimaryBonusPoolGroupFromTemplate
{
    public static Dictionary<string, double> SummarizeAdjustmentTemplatesForGroup(List<AdjustmentTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<PrimaryBonusPoolGroup> DerivePrimaryBonusPoolGroupsFromTemplate(List<AdjustmentTemplate> items)
    {
        var outValue = new List<PrimaryBonusPoolGroup>();
        foreach (var item in items)
        {
            var derived = PrimaryBonusPoolGroup.NewPrimaryBonusPoolGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePrimaryBonusPoolGroup(PrimaryBonusPoolGroup item)
    {
        return AdjustmentScorePolicy.ClassifyAdjustmentScore(item.NetValue());
    }
}
