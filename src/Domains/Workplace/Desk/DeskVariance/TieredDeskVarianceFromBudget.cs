namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Functions of tiered desk variance from budget.</summary>
public static class TieredDeskVarianceFromBudget
{
    public static Dictionary<string, double> SummarizeManualAccessBadgeBudgetsForVariance(List<ManualAccessBadgeBudget> items)
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

    public static List<TieredDeskVariance> DeriveTieredDeskVariancesFromBudget(List<ManualAccessBadgeBudget> items)
    {
        var outValue = new List<TieredDeskVariance>();
        foreach (var item in items)
        {
            var derived = TieredDeskVariance.NewTieredDeskVariance(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreTieredDeskVariance(TieredDeskVariance item)
    {
        return ScheduledAccessBadgeReservationPolicy.ClassifyScheduledAccessBadgeReservation(item.AdjustedTotal());
    }
}
