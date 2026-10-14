namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Functions of manual equity grant group from plan.</summary>
public static class ManualEquityGrantGroupFromPlan
{
    public static Dictionary<string, double> SummarizeTieredMeritCyclePlansForGroup(List<TieredMeritCyclePlan> items)
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

    public static List<ManualEquityGrantGroup> DeriveManualEquityGrantGroupsFromPlan(List<TieredMeritCyclePlan> items)
    {
        var outValue = new List<ManualEquityGrantGroup>();
        foreach (var item in items)
        {
            var derived = ManualEquityGrantGroup.NewManualEquityGrantGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanManualEquityGrantGroup(ManualEquityGrantGroup item)
    {
        return SeasonalMeritCycleReservationPolicy.ClassifySeasonalMeritCycleReservation(item.NetLoad());
    }
}
