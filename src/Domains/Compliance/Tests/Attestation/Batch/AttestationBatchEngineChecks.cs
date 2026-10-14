namespace Acme.Workforce.Compliance.Tests.Attestation.Batch;

using Acme.Workforce.Compliance.Attestation.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AttestationBatchEngineChecks
{
    public static void AttestationBatchEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationBatchEngine.DefaultAttestationBatchEngine(Flags.NewRegistry());
        var item = AttestationBatch.NewAttestationBatch("code-454", "name-300");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationBatchEngineKeepsResultsWithinBounds();
    }
}
