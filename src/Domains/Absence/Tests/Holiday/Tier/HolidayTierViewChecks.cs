namespace Acme.Workforce.Absence.Tests.Holiday.Tier;

using Acme.Workforce.Absence.Holiday.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HolidayTierViewChecks
{
    public static void HolidayTierViewShowsTheStatusInCapitals()
    {
        var item = HolidayTier.NewHolidayTier("code-192", "title-135");
        var view = HolidayTierView.ToHolidayTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayTierView.ToHolidayTierViews(new List<HolidayTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayTierViewShowsTheStatusInCapitals();
    }
}
