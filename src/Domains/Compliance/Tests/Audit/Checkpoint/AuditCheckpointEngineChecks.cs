namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditCheckpointEngineChecks
{
    public static void AuditCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = AuditCheckpointEngine.DefaultAuditCheckpointEngine(Flags.NewRegistry());
        var item = AuditCheckpoint.NewAuditCheckpoint("code-551", "name-827");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditCheckpointEngineKeepsResultsWithinBounds();
    }
}
