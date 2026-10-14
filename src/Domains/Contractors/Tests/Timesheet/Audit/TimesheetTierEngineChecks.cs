namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class TimesheetTierEngineChecks
{
    public static void TimesheetTierEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetTierEngine.DefaultTimesheetTierEngine(Flags.NewRegistry());
        var item = TimesheetTier.NewTimesheetTier("code-368", "title-933");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetTierEngineKeepsResultsWithinBounds();
    }
}
