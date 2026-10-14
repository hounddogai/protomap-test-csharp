namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class HolidayRequestViewChecks
{
    public static void HolidayRequestViewShowsTheStatusInCapitals()
    {
        var item = HolidayRequest.NewHolidayRequest("code-964", "name-882");
        var view = HolidayRequestView.ToHolidayRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayRequestView.ToHolidayRequestViews(new List<HolidayRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayRequestViewShowsTheStatusInCapitals();
    }
}
