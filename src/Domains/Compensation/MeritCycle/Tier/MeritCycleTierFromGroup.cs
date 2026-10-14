namespace Acme.Workforce.Compensation.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Functions of merit cycle tier from group.</summary>
public static class MeritCycleTierFromGroup
{
    public static Dictionary<string, double> SummarizeMeritCycleGroupsForTier(List<MeritCycleGroup> items)
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

    public static List<MeritCycleTier> DeriveMeritCycleTiersFromGroup(List<MeritCycleGroup> items)
    {
        var outValue = new List<MeritCycleTier>();
        foreach (var item in items)
        {
            var derived = MeritCycleTier.NewMeritCycleTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankMeritCycleTier(MeritCycleTier item)
    {
        return MeritCycleGroupPolicy.NormalizeMeritCycleGroupCode(item.Code);
    }
}
