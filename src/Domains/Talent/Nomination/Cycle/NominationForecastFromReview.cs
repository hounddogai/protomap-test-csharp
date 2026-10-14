namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Functions of nomination forecast from review.</summary>
public static class NominationForecastFromReview
{
    public static Dictionary<string, double> SummarizeMobilityReviewsForForecast(List<MobilityReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<NominationForecast> DeriveNominationForecastsFromReview(List<MobilityReview> items)
    {
        var outValue = new List<NominationForecast>();
        foreach (var item in items)
        {
            var derived = NominationForecast.NewNominationForecast(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeNominationForecast(NominationForecast item)
    {
        return MobilitySegmentPolicy.MobilitySegmentBucket(item.EffectiveLoad());
    }
}
