namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Functions of tiered desk cycle from reservation.</summary>
public static class TieredDeskCycleFromReservation
{
    public static Dictionary<string, double> SummarizePrimaryFloorPlanReservationsForCycle(List<PrimaryFloorPlanReservation> items)
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

    public static List<TieredDeskCycle> DeriveTieredDeskCyclesFromReservation(List<PrimaryFloorPlanReservation> items)
    {
        var outValue = new List<TieredDeskCycle>();
        foreach (var item in items)
        {
            var derived = TieredDeskCycle.NewTieredDeskCycle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanTieredDeskCycle(TieredDeskCycle item)
    {
        return CappedFloorPlanThresholdPolicy.NormalizeCappedFloorPlanThresholdCode(item.Code);
    }
}
