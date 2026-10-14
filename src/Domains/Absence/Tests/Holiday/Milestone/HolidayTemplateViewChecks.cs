namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayTemplateViewChecks
{
    public static void HolidayTemplateViewShowsTheStatusInCapitals()
    {
        var item = HolidayTemplate.NewHolidayTemplate("code-459", "title-800");
        var view = HolidayTemplateView.ToHolidayTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayTemplateView.ToHolidayTemplateViews(new List<HolidayTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayTemplateViewShowsTheStatusInCapitals();
    }
}
