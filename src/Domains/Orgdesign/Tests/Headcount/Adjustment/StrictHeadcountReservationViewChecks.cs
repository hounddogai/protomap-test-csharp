namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictHeadcountReservationViewChecks
{
    public static void StrictHeadcountReservationViewShowsTheStatusInCapitals()
    {
        var item = StrictHeadcountReservation.NewStrictHeadcountReservation("code-708", "label-448");
        var view = StrictHeadcountReservationView.ToStrictHeadcountReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictHeadcountReservationView.ToStrictHeadcountReservationViews(new List<StrictHeadcountReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictHeadcountReservationViewShowsTheStatusInCapitals();
    }
}
