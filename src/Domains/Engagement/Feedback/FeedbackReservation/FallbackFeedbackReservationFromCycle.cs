namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Functions of fallback feedback reservation from cycle.</summary>
public static class FallbackFeedbackReservationFromCycle
{
    public static Dictionary<string, double> SummarizeLegacyTownHallCyclesForReservation(List<LegacyTownHallCycle> items)
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

    public static List<FallbackFeedbackReservation> DeriveFallbackFeedbackReservationsFromCycle(List<LegacyTownHallCycle> items)
    {
        var outValue = new List<FallbackFeedbackReservation>();
        foreach (var item in items)
        {
            var derived = FallbackFeedbackReservation.NewFallbackFeedbackReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectFallbackFeedbackReservation(FallbackFeedbackReservation item)
    {
        return LegacyTownHallSchedulePolicy.ClassifyLegacyTownHallSchedule(item.AdjustedVolume());
    }
}
