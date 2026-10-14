namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class EngagementReservationViewChecks
{
    public static void EngagementReservationViewShowsTheStatusInCapitals()
    {
        var item = EngagementReservation.NewEngagementReservation("code-761", "title-287");
        var view = EngagementReservationView.ToEngagementReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementReservationView.ToEngagementReservationViews(new List<EngagementReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementReservationViewShowsTheStatusInCapitals();
    }
}
