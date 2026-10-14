namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledDeskSummaryEngineChecks
{
    public static void ScheduledDeskSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledDeskSummaryEngine.DefaultScheduledDeskSummaryEngine(Flags.NewRegistry());
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("code-698", "title-469");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledDeskSummaryEngineKeepsResultsWithinBounds();
    }
}
