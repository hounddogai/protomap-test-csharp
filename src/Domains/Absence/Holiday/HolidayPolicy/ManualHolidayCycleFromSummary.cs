namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Functions of manual holiday cycle from summary.</summary>
public static class ManualHolidayCycleFromSummary
{
    public static Dictionary<string, double> SummarizeReturnPlanSummariesForCycle(List<ReturnPlanSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<ManualHolidayCycle> DeriveManualHolidayCyclesFromSummary(List<ReturnPlanSummary> items)
    {
        var outValue = new List<ManualHolidayCycle>();
        foreach (var item in items)
        {
            var derived = ManualHolidayCycle.NewManualHolidayCycle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeManualHolidayCycle(ManualHolidayCycle item)
    {
        return LenientReturnPlanOverridePolicy.LenientReturnPlanOverrideBucket(item.AdjustedTotal());
    }
}
