namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyReservationViewChecks
{
    public static void PolicyReservationViewShowsTheStatusInCapitals()
    {
        var item = PolicyReservation.NewPolicyReservation("code-261", "name-627");
        var view = PolicyReservationView.ToPolicyReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyReservationView.ToPolicyReservationViews(new List<PolicyReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyReservationViewShowsTheStatusInCapitals();
    }
}
