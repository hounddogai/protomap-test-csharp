namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Functions of strict headcount reservation from forecast.</summary>
public static class StrictHeadcountReservationFromForecast
{
    public static Dictionary<string, double> SummarizePositionForecastsForReservation(List<PositionForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<StrictHeadcountReservation> DeriveStrictHeadcountReservationsFromForecast(List<PositionForecast> items)
    {
        var outValue = new List<StrictHeadcountReservation>();
        foreach (var item in items)
        {
            var derived = StrictHeadcountReservation.NewStrictHeadcountReservation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeStrictHeadcountReservation(StrictHeadcountReservation item)
    {
        return PositionForecastPolicy.PositionForecastBucket(item.EffectiveExposure());
    }
}
