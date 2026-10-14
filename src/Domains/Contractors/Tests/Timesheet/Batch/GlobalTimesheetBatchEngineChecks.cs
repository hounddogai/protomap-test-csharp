namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalTimesheetBatchEngineChecks
{
    public static void GlobalTimesheetBatchEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalTimesheetBatchEngine.DefaultGlobalTimesheetBatchEngine();
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("code-206", "title-339");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalTimesheetBatchEngineKeepsResultsWithinBounds();
    }
}
