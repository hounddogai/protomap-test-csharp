namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedTimesheetTierEngineChecks
{
    public static void BatchedTimesheetTierEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedTimesheetTierEngine.DefaultBatchedTimesheetTierEngine(Flags.NewRegistry());
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("code-635", "label-204");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedTimesheetTierEngineKeepsResultsWithinBounds();
    }
}
