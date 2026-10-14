namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultBonusPoolReservationViewChecks
{
    public static void DefaultBonusPoolReservationViewShowsTheStatusInCapitals()
    {
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("code-458", "name-11");
        var view = DefaultBonusPoolReservationView.ToDefaultBonusPoolReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultBonusPoolReservationView.ToDefaultBonusPoolReservationViews(new List<DefaultBonusPoolReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultBonusPoolReservationViewShowsTheStatusInCapitals();
    }
}
