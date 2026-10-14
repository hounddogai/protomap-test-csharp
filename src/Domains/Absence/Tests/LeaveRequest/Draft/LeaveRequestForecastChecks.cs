namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestForecastChecks
{
    public static void LeaveRequestForecastStartsInCreated()
    {
        var item = LeaveRequestForecast.NewLeaveRequestForecast("code-232", "title-275");
        Check.Equal(item.Status, LeaveRequestForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestForecastMovesToValidated()
    {
        var item = LeaveRequestForecast.NewLeaveRequestForecast("code-232", "title-275");
        try
        {
            item.MoveTo(LeaveRequestForecastStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestForecastStatus.Validated, "status");
    }

    public static void LeaveRequestForecastRejectsMovesFromRetired()
    {
        var item = LeaveRequestForecast.NewLeaveRequestForecast("code-232", "title-275");
        item.Status = LeaveRequestForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(LeaveRequestForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        LeaveRequestForecastStartsInCreated();
        LeaveRequestForecastMovesToValidated();
        LeaveRequestForecastRejectsMovesFromRetired();
    }
}
