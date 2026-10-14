namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountSchedule;

using Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

/// <summary>Checks of headcount schedule code, run by the project's runner.</summary>
public static class SeasonalHeadcountReservationViewChecks
{
    public static void SeasonalHeadcountReservationViewShowsTheStatusInCapitals()
    {
        var item = SeasonalHeadcountReservation.NewSeasonalHeadcountReservation("code-430", "title-691");
        var view = SeasonalHeadcountReservationView.ToSeasonalHeadcountReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalHeadcountReservationView.ToSeasonalHeadcountReservationViews(new List<SeasonalHeadcountReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalHeadcountReservationViewShowsTheStatusInCapitals();
    }
}
