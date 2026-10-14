namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Functions of pay band review from forecast.</summary>
public static class PayBandReviewFromForecast
{
    public static Dictionary<string, double> SummarizeBenchmarkForecastsForReview(List<BenchmarkForecast> items)
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

    public static List<PayBandReview> DerivePayBandReviewsFromForecast(List<BenchmarkForecast> items)
    {
        var outValue = new List<PayBandReview>();
        foreach (var item in items)
        {
            var derived = PayBandReview.NewPayBandReview(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignPayBandReview(PayBandReview item)
    {
        return BenchmarkForecastPolicy.BenchmarkForecastWeight(6, true);
    }
}
