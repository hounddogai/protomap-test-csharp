namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientLeaveRequestForecastViewChecks
{
    public static void LenientLeaveRequestForecastViewShowsTheStatusInCapitals()
    {
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("code-718", "label-192");
        var view = LenientLeaveRequestForecastView.ToLenientLeaveRequestForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientLeaveRequestForecastView.ToLenientLeaveRequestForecastViews(new List<LenientLeaveRequestForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientLeaveRequestForecastViewShowsTheStatusInCapitals();
    }
}
