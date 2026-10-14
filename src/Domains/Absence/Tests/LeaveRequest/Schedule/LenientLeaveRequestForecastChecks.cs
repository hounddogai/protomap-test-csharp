namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientLeaveRequestForecastChecks
{
    public static void LenientLeaveRequestForecastStartsInPlanned()
    {
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("code-718", "label-192");
        Check.Equal(item.Status, LenientLeaveRequestForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLeaveRequestForecastMovesToCompleted()
    {
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("code-718", "label-192");
        try
        {
            item.MoveTo(LenientLeaveRequestForecastStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLeaveRequestForecastStatus.Completed, "status");
    }

    public static void LenientLeaveRequestForecastRejectsMovesFromCancelled()
    {
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("code-718", "label-192");
        item.Status = LenientLeaveRequestForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientLeaveRequestForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientLeaveRequestForecastStartsInPlanned();
        LenientLeaveRequestForecastMovesToCompleted();
        LenientLeaveRequestForecastRejectsMovesFromCancelled();
    }
}
