namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingAccessBadgeBatchEngineChecks
{
    public static void RollingAccessBadgeBatchEngineKeepsResultsWithinBounds()
    {
        var engine = RollingAccessBadgeBatchEngine.DefaultRollingAccessBadgeBatchEngine(Flags.NewRegistry());
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("code-701", "title-332");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingAccessBadgeBatchEngineKeepsResultsWithinBounds();
    }
}
