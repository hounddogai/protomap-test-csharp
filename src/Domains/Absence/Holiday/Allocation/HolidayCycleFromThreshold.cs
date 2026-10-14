namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Functions of holiday cycle from threshold.</summary>
public static class HolidayCycleFromThreshold
{
    public static Dictionary<string, double> SummarizeTieredReturnPlanThresholdsForCycle(List<TieredReturnPlanThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<HolidayCycle> DeriveHolidayCyclesFromThreshold(List<TieredReturnPlanThreshold> items)
    {
        var outValue = new List<HolidayCycle>();
        foreach (var item in items)
        {
            var derived = HolidayCycle.NewHolidayCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeHolidayCycle(HolidayCycle item)
    {
        return WeightedReturnPlanManifestPolicy.ClassifyWeightedReturnPlanManifest(item.MeasuredValue());
    }
}
