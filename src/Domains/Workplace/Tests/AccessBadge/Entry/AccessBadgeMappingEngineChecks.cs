namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMappingEngineChecks
{
    public static void AccessBadgeMappingEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeMappingEngine.DefaultAccessBadgeMappingEngine(Flags.NewRegistry());
        var item = AccessBadgeMapping.NewAccessBadgeMapping("code-779", "name-618");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeMappingEngineKeepsResultsWithinBounds();
    }
}
