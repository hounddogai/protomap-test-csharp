namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AdaptiveTimesheetDraftEngineChecks
{
    public static void AdaptiveTimesheetDraftEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTimesheetDraftEngine.DefaultAdaptiveTimesheetDraftEngine(Flags.NewRegistry());
        var item = AdaptiveTimesheetDraft.NewAdaptiveTimesheetDraft("code-242", "name-906");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTimesheetDraftEngineKeepsResultsWithinBounds();
    }
}
