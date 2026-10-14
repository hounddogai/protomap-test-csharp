namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Functions of rolling merit cycle target from slot.</summary>
public static class RollingMeritCycleTargetFromSlot
{
    public static Dictionary<string, double> SummarizeRegionalEquityGrantSlotsForTarget(List<RegionalEquityGrantSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RollingMeritCycleTarget> DeriveRollingMeritCycleTargetsFromSlot(List<RegionalEquityGrantSlot> items)
    {
        var outValue = new List<RollingMeritCycleTarget>();
        foreach (var item in items)
        {
            var derived = RollingMeritCycleTarget.NewRollingMeritCycleTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRollingMeritCycleTarget(RollingMeritCycleTarget item)
    {
        return AdaptiveEquityGrantTargetPolicy.ClassifyAdaptiveEquityGrantTarget(item.AdjustedLoad());
    }
}
