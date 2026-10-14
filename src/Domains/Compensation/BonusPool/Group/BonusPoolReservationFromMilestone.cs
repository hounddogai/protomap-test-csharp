namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Functions of bonus pool reservation from milestone.</summary>
public static class BonusPoolReservationFromMilestone
{
    public static Dictionary<string, double> SummarizeWeightedBonusPoolMilestonesForReservation(List<WeightedBonusPoolMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<BonusPoolReservation> DeriveBonusPoolReservationsFromMilestone(List<WeightedBonusPoolMilestone> items)
    {
        var outValue = new List<BonusPoolReservation>();
        foreach (var item in items)
        {
            var derived = BonusPoolReservation.NewBonusPoolReservation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitBonusPoolReservation(BonusPoolReservation item)
    {
        return FallbackBonusPoolMilestonePolicy.FallbackBonusPoolMilestoneWeight(3, true);
    }
}
