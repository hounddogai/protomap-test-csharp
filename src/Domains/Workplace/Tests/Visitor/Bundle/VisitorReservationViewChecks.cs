namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorReservationViewChecks
{
    public static void VisitorReservationViewShowsTheStatusInCapitals()
    {
        var item = VisitorReservation.NewVisitorReservation("code-720", "title-944");
        var view = VisitorReservationView.ToVisitorReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorReservationView.ToVisitorReservationViews(new List<VisitorReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorReservationViewShowsTheStatusInCapitals();
    }
}
