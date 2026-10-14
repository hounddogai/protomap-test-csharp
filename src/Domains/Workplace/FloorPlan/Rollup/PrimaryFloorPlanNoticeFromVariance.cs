namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Functions of primary floor plan notice from variance.</summary>
public static class PrimaryFloorPlanNoticeFromVariance
{
    public static Dictionary<string, double> SummarizeSeasonalFloorPlanVariancesForNotice(List<SeasonalFloorPlanVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PrimaryFloorPlanNotice> DerivePrimaryFloorPlanNoticesFromVariance(List<SeasonalFloorPlanVariance> items)
    {
        var outValue = new List<PrimaryFloorPlanNotice>();
        foreach (var item in items)
        {
            var derived = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizePrimaryFloorPlanNotice(PrimaryFloorPlanNotice item)
    {
        return SeasonalFloorPlanCheckpointPolicy.SeasonalFloorPlanCheckpointWeight(4, true);
    }
}
