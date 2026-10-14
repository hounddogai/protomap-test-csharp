namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Functions of pending booking plan from cycle.</summary>
public static class PendingBookingPlanFromCycle
{
    public static Dictionary<string, double> SummarizeTieredDeskCyclesForPlan(List<TieredDeskCycle> items)
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

    public static List<PendingBookingPlan> DerivePendingBookingPlansFromCycle(List<TieredDeskCycle> items)
    {
        var outValue = new List<PendingBookingPlan>();
        foreach (var item in items)
        {
            var derived = PendingBookingPlan.NewPendingBookingPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPendingBookingPlan(PendingBookingPlan item)
    {
        return TieredDeskCyclePolicy.TieredDeskCycleBucket(item.EffectiveVolume());
    }
}
