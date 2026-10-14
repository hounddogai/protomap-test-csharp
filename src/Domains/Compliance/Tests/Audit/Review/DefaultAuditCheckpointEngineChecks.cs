namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultAuditCheckpointEngineChecks
{
    public static void DefaultAuditCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAuditCheckpointEngine.DefaultDefaultAuditCheckpointEngine(Flags.NewRegistry());
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("code-816", "name-623");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAuditCheckpointEngineKeepsResultsWithinBounds();
    }
}
