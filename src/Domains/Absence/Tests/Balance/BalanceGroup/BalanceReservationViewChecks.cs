namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalanceReservationViewChecks
{
    public static void BalanceReservationViewShowsTheStatusInCapitals()
    {
        var item = BalanceReservation.NewBalanceReservation("code-418", "title-341");
        var view = BalanceReservationView.ToBalanceReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceReservationView.ToBalanceReservationViews(new List<BalanceReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceReservationViewShowsTheStatusInCapitals();
    }
}
