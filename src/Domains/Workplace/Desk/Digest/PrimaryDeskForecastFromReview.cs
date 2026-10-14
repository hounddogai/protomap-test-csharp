namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Functions of primary desk forecast from review.</summary>
public static class PrimaryDeskForecastFromReview
{
    public static Dictionary<string, double> SummarizeOccupancyReviewsForForecast(List<OccupancyReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PrimaryDeskForecast> DerivePrimaryDeskForecastsFromReview(List<OccupancyReview> items)
    {
        var outValue = new List<PrimaryDeskForecast>();
        foreach (var item in items)
        {
            var derived = PrimaryDeskForecast.NewPrimaryDeskForecast(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizePrimaryDeskForecast(PrimaryDeskForecast item)
    {
        return OccupancyBatchPolicy.OccupancyBatchBucket(item.MeasuredValue());
    }
}
