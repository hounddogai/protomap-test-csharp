namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Functions of merit cycle forecast from template.</summary>
public static class MeritCycleForecastFromTemplate
{
    public static Dictionary<string, double> SummarizePayBandTemplatesForForecast(List<PayBandTemplate> items)
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

    public static List<MeritCycleForecast> DeriveMeritCycleForecastsFromTemplate(List<PayBandTemplate> items)
    {
        var outValue = new List<MeritCycleForecast>();
        foreach (var item in items)
        {
            var derived = MeritCycleForecast.NewMeritCycleForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanMeritCycleForecast(MeritCycleForecast item)
    {
        return PayBandTemplatePolicy.PayBandTemplateWeight(7, true);
    }
}
