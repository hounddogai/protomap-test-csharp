namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Functions of primary desk forecast from budget.</summary>
public static class PrimaryDeskForecastFromBudget
{
    public static Dictionary<string, double> SummarizeStrictDeskBudgetsForForecast(List<StrictDeskBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<PrimaryDeskForecast> DerivePrimaryDeskForecastsFromBudget(List<StrictDeskBudget> items)
    {
        var outValue = new List<PrimaryDeskForecast>();
        foreach (var item in items)
        {
            var derived = PrimaryDeskForecast.NewPrimaryDeskForecast(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchPrimaryDeskForecast(PrimaryDeskForecast item)
    {
        return DeskBatchPolicy.DeskBatchWeight(4, true);
    }
}
