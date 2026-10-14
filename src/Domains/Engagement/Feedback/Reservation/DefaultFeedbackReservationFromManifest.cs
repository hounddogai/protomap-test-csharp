namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Functions of default feedback reservation from manifest.</summary>
public static class DefaultFeedbackReservationFromManifest
{
    public static Dictionary<string, double> SummarizeActionPlanManifestsForReservation(List<ActionPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<DefaultFeedbackReservation> DeriveDefaultFeedbackReservationsFromManifest(List<ActionPlanManifest> items)
    {
        var outValue = new List<DefaultFeedbackReservation>();
        foreach (var item in items)
        {
            var derived = DefaultFeedbackReservation.NewDefaultFeedbackReservation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateDefaultFeedbackReservation(DefaultFeedbackReservation item)
    {
        return ActionPlanManifestPolicy.NormalizeActionPlanManifestCode(item.Code);
    }
}
