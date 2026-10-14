namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedEngagementReservationViewChecks
{
    public static void WeightedEngagementReservationViewShowsTheStatusInCapitals()
    {
        var item = WeightedEngagementReservation.NewWeightedEngagementReservation("code-721", "label-107");
        var view = WeightedEngagementReservationView.ToWeightedEngagementReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedEngagementReservationView.ToWeightedEngagementReservationViews(new List<WeightedEngagementReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedEngagementReservationViewShowsTheStatusInCapitals();
    }
}
