namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Functions of pay band forecast from reservation.</summary>
public static class PayBandForecastFromReservation
{
    public static Dictionary<string, double> SummarizeAdjustmentReservationsForForecast(List<AdjustmentReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PayBandForecast> DerivePayBandForecastsFromReservation(List<AdjustmentReservation> items)
    {
        var outValue = new List<PayBandForecast>();
        foreach (var item in items)
        {
            var derived = PayBandForecast.NewPayBandForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPayBandForecast(PayBandForecast item)
    {
        return AdjustmentNoticePolicy.ClassifyAdjustmentNotice(item.EffectiveVolume());
    }
}
