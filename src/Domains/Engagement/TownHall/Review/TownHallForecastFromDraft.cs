namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Functions of town hall forecast from draft.</summary>
public static class TownHallForecastFromDraft
{
    public static Dictionary<string, double> SummarizeFeedbackDraftsForForecast(List<FeedbackDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<TownHallForecast> DeriveTownHallForecastsFromDraft(List<FeedbackDraft> items)
    {
        var outValue = new List<TownHallForecast>();
        foreach (var item in items)
        {
            var derived = TownHallForecast.NewTownHallForecast(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeTownHallForecast(TownHallForecast item)
    {
        return FeedbackExceptionPolicy.FeedbackExceptionWeight(1, true);
    }
}
