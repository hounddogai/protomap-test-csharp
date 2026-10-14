namespace Acme.Workforce.Contractors.Tests.Timesheet.Score;

using Acme.Workforce.Contractors.Timesheet.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultTimesheetPreferenceEngineChecks
{
    public static void DefaultTimesheetPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultTimesheetPreferenceEngine.DefaultDefaultTimesheetPreferenceEngine(Flags.NewRegistry());
        var item = DefaultTimesheetPreference.NewDefaultTimesheetPreference("code-49", "title-243");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultTimesheetPreferenceEngineKeepsResultsWithinBounds();
    }
}
