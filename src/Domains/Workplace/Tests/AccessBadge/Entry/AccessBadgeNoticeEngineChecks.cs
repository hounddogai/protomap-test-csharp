namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeNoticeEngineChecks
{
    public static void AccessBadgeNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeNoticeEngine.DefaultAccessBadgeNoticeEngine();
        var item = AccessBadgeNotice.NewAccessBadgeNotice("code-850", "title-260");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeNoticeEngineKeepsResultsWithinBounds();
    }
}
