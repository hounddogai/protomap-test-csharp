namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PoolReservationViewChecks
{
    public static void PoolReservationViewShowsTheStatusInCapitals()
    {
        var item = PoolReservation.NewPoolReservation("code-725", "label-924");
        var view = PoolReservationView.ToPoolReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolReservationView.ToPoolReservationViews(new List<PoolReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolReservationViewShowsTheStatusInCapitals();
    }
}
