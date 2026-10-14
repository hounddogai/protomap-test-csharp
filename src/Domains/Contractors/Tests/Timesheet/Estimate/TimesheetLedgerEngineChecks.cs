namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TimesheetLedgerEngineChecks
{
    public static void TimesheetLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetLedgerEngine.DefaultTimesheetLedgerEngine(Flags.NewRegistry());
        var item = TimesheetLedger.NewTimesheetLedger("code-949", "label-107");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetLedgerEngineKeepsResultsWithinBounds();
    }
}
