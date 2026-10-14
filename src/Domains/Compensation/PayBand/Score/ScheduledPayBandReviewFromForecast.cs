namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Functions of scheduled pay band review from forecast.</summary>
public static class ScheduledPayBandReviewFromForecast
{
    public static Dictionary<string, double> SummarizePrimaryPayBandForecastsForReview(List<PrimaryPayBandForecast> items)
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

    public static List<ScheduledPayBandReview> DeriveScheduledPayBandReviewsFromForecast(List<PrimaryPayBandForecast> items)
    {
        var outValue = new List<ScheduledPayBandReview>();
        foreach (var item in items)
        {
            var derived = ScheduledPayBandReview.NewScheduledPayBandReview(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyScheduledPayBandReview(ScheduledPayBandReview item)
    {
        return PayBandCyclePolicy.PayBandCycleWeight(9, true);
    }
}
