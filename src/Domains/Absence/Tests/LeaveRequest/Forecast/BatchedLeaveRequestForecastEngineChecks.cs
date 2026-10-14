namespace Acme.Workforce.Absence.Tests.LeaveRequest.Forecast;

using Acme.Workforce.Absence.LeaveRequest.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class BatchedLeaveRequestForecastEngineChecks
{
    public static void BatchedLeaveRequestForecastEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedLeaveRequestForecastEngine.DefaultBatchedLeaveRequestForecastEngine();
        var item = BatchedLeaveRequestForecast.NewBatchedLeaveRequestForecast("code-971", "name-691");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedLeaveRequestForecastEngineKeepsResultsWithinBounds();
    }
}
