namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class DefaultAccessBadgeRollupEngineChecks
{
    public static void DefaultAccessBadgeRollupEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAccessBadgeRollupEngine.DefaultDefaultAccessBadgeRollupEngine(Flags.NewRegistry());
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("code-16", "label-466");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeRollupEngineKeepsResultsWithinBounds();
    }
}
