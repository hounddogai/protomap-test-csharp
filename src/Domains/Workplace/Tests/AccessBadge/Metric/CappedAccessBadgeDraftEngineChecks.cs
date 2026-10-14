namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedAccessBadgeDraftEngineChecks
{
    public static void CappedAccessBadgeDraftEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAccessBadgeDraftEngine.DefaultCappedAccessBadgeDraftEngine();
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("code-412", "label-655");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAccessBadgeDraftEngineKeepsResultsWithinBounds();
    }
}
