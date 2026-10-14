namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>Functions of equity grant budget from reservation.</summary>
public static class EquityGrantBudgetFromReservation
{
    public static Dictionary<string, double> SummarizeMeritCycleReservationsForBudget(List<MeritCycleReservation> items)
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

    public static List<EquityGrantBudget> DeriveEquityGrantBudgetsFromReservation(List<MeritCycleReservation> items)
    {
        var outValue = new List<EquityGrantBudget>();
        foreach (var item in items)
        {
            var derived = EquityGrantBudget.NewEquityGrantBudget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeEquityGrantBudget(EquityGrantBudget item)
    {
        return MeritCycleForecastPolicy.MeritCycleForecastBucket(item.MeasuredValue());
    }
}
