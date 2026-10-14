namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayExceptionViewChecks
{
    public static void HolidayExceptionViewShowsTheStatusInCapitals()
    {
        var item = HolidayException.NewHolidayException("code-672", "title-760");
        var view = HolidayExceptionView.ToHolidayExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayExceptionView.ToHolidayExceptionViews(new List<HolidayException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayExceptionViewShowsTheStatusInCapitals();
    }
}
