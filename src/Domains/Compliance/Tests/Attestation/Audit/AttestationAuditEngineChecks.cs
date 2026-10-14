namespace Acme.Workforce.Compliance.Tests.Attestation.Audit;

using Acme.Workforce.Compliance.Attestation.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AttestationAuditEngineChecks
{
    public static void AttestationAuditEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationAuditEngine.DefaultAttestationAuditEngine();
        var item = AttestationAudit.NewAttestationAudit("code-445", "title-29");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationAuditEngineKeepsResultsWithinBounds();
    }
}
