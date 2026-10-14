namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Functions of rolling bonus pool slot from profile.</summary>
public static class RollingBonusPoolSlotFromProfile
{
    public static Dictionary<string, double> SummarizeSeasonalMeritCycleProfilesForSlot(List<SeasonalMeritCycleProfile> items)
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

    public static List<RollingBonusPoolSlot> DeriveRollingBonusPoolSlotsFromProfile(List<SeasonalMeritCycleProfile> items)
    {
        var outValue = new List<RollingBonusPoolSlot>();
        foreach (var item in items)
        {
            var derived = RollingBonusPoolSlot.NewRollingBonusPoolSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRollingBonusPoolSlot(RollingBonusPoolSlot item)
    {
        return PrimaryMeritCycleSlotPolicy.ClassifyPrimaryMeritCycleSlot(item.NetLoad());
    }
}
