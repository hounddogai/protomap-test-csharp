namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Functions of weighted laptop estimate from reservation.</summary>
public static class WeightedLaptopEstimateFromReservation
{
    public static Dictionary<string, double> SummarizeWeightedRefreshReservationsForEstimate(List<WeightedRefreshReservation> items)
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

    public static List<WeightedLaptopEstimate> DeriveWeightedLaptopEstimatesFromReservation(List<WeightedRefreshReservation> items)
    {
        var outValue = new List<WeightedLaptopEstimate>();
        foreach (var item in items)
        {
            var derived = WeightedLaptopEstimate.NewWeightedLaptopEstimate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignWeightedLaptopEstimate(WeightedLaptopEstimate item)
    {
        return RefreshOverridePolicy.ClassifyRefreshOverride(item.ProjectedVolume());
    }
}
