namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Functions of equity grant review from window.</summary>
public static class EquityGrantReviewFromWindow
{
    public static Dictionary<string, double> SummarizeMeritCycleWindowsForReview(List<MeritCycleWindow> items)
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

    public static List<EquityGrantReview> DeriveEquityGrantReviewsFromWindow(List<MeritCycleWindow> items)
    {
        var outValue = new List<EquityGrantReview>();
        foreach (var item in items)
        {
            var derived = EquityGrantReview.NewEquityGrantReview(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreEquityGrantReview(EquityGrantReview item)
    {
        return MeritCycleProfilePolicy.ClassifyMeritCycleProfile(item.NetLoad());
    }
}
