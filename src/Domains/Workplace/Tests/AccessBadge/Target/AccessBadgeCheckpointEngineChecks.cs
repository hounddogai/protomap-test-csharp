namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeCheckpointEngineChecks
{
    public static void AccessBadgeCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeCheckpointEngine.DefaultAccessBadgeCheckpointEngine(Flags.NewRegistry());
        var item = AccessBadgeCheckpoint.NewAccessBadgeCheckpoint("code-208", "name-308");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeCheckpointEngineKeepsResultsWithinBounds();
    }
}
