namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class WeightedTimesheetMetricEngineChecks
{
    public static void WeightedTimesheetMetricEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedTimesheetMetricEngine.DefaultWeightedTimesheetMetricEngine();
        var item = WeightedTimesheetMetric.NewWeightedTimesheetMetric("code-227", "title-100");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedTimesheetMetricEngineKeepsResultsWithinBounds();
    }
}
