namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeSummaryEngineChecks
{
    public static void AccessBadgeSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeSummaryEngine.DefaultAccessBadgeSummaryEngine();
        var item = AccessBadgeSummary.NewAccessBadgeSummary("code-592", "name-680");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeSummaryEngineKeepsResultsWithinBounds();
    }
}
