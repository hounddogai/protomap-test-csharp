namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultAccessBadgeOverrideEngineChecks
{
    public static void DefaultAccessBadgeOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAccessBadgeOverrideEngine.DefaultDefaultAccessBadgeOverrideEngine(Flags.NewRegistry());
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("code-529", "title-767");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeOverrideEngineKeepsResultsWithinBounds();
    }
}
