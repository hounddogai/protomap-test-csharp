namespace Acme.Workforce.Compliance.Tests.Attestation.Rollup;

using Acme.Workforce.Compliance.Attestation.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AttestationRecordEngineChecks
{
    public static void AttestationRecordEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationRecordEngine.DefaultAttestationRecordEngine(Flags.NewRegistry());
        var item = AttestationRecord.NewAttestationRecord("code-428", "name-765");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationRecordEngineKeepsResultsWithinBounds();
    }
}
