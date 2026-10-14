namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Functions of seasonal floor plan variance from draft.</summary>
public static class SeasonalFloorPlanVarianceFromDraft
{
    public static Dictionary<string, double> SummarizeDeskDraftsForVariance(List<DeskDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<SeasonalFloorPlanVariance> DeriveSeasonalFloorPlanVariancesFromDraft(List<DeskDraft> items)
    {
        var outValue = new List<SeasonalFloorPlanVariance>();
        foreach (var item in items)
        {
            var derived = SeasonalFloorPlanVariance.NewSeasonalFloorPlanVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveSeasonalFloorPlanVariance(SeasonalFloorPlanVariance item)
    {
        return LegacyDeskReservationPolicy.LegacyDeskReservationWeight(7, true);
    }
}
