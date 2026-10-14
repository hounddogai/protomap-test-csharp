namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedAccessBadgeThresholdEngineChecks
{
    public static void CappedAccessBadgeThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAccessBadgeThresholdEngine.DefaultCappedAccessBadgeThresholdEngine(Flags.NewRegistry());
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("code-931", "title-878");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAccessBadgeThresholdEngineKeepsResultsWithinBounds();
    }
}
