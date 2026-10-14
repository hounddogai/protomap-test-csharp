namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountReservationViewChecks
{
    public static void HeadcountReservationViewShowsTheStatusInCapitals()
    {
        var item = HeadcountReservation.NewHeadcountReservation("code-390", "title-713");
        var view = HeadcountReservationView.ToHeadcountReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountReservationView.ToHeadcountReservationViews(new List<HeadcountReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountReservationViewShowsTheStatusInCapitals();
    }
}
