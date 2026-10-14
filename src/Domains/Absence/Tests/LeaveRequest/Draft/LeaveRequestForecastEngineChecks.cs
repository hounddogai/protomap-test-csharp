namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestForecastEngineChecks
{
    public static void LeaveRequestForecastEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestForecastEngine.DefaultLeaveRequestForecastEngine();
        var item = LeaveRequestForecast.NewLeaveRequestForecast("code-232", "title-275");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestForecastEngineKeepsResultsWithinBounds();
    }
}
