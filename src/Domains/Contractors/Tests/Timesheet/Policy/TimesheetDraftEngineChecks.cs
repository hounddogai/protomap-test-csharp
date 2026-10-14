namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetDraftEngineChecks
{
    public static void TimesheetDraftEngineKeepsResultsWithinBounds()
    {
        var engine = TimesheetDraftEngine.DefaultTimesheetDraftEngine(Flags.NewRegistry());
        var item = TimesheetDraft.NewTimesheetDraft("code-799", "label-733");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TimesheetDraftEngineKeepsResultsWithinBounds();
    }
}
