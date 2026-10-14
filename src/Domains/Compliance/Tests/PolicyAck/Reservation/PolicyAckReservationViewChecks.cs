namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PolicyAckReservationViewChecks
{
    public static void PolicyAckReservationViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckReservation.NewPolicyAckReservation("code-930", "title-430");
        var view = PolicyAckReservationView.ToPolicyAckReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckReservationView.ToPolicyAckReservationViews(new List<PolicyAckReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckReservationViewShowsTheStatusInCapitals();
    }
}
