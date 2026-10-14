namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Workplace.Desk.DeskVariance;

/// <summary>Functions of floor plan quota from variance.</summary>
public static class FloorPlanQuotaFromVariance
{
    public static Dictionary<string, double> SummarizeTieredDeskVariancesForQuota(List<TieredDeskVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<FloorPlanQuota> DeriveFloorPlanQuotasFromVariance(List<TieredDeskVariance> items)
    {
        var outValue = new List<FloorPlanQuota>();
        foreach (var item in items)
        {
            var derived = FloorPlanQuota.NewFloorPlanQuota(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceFloorPlanQuota(FloorPlanQuota item)
    {
        return DeskNoticePolicy.DeskNoticeBucket(item.WeightedVolume());
    }
}
