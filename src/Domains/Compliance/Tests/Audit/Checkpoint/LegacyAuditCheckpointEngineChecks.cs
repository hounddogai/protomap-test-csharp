namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyAuditCheckpointEngineChecks
{
    public static void LegacyAuditCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyAuditCheckpointEngine.DefaultLegacyAuditCheckpointEngine();
        var item = LegacyAuditCheckpoint.NewLegacyAuditCheckpoint("code-664", "title-96");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyAuditCheckpointEngineKeepsResultsWithinBounds();
    }
}
