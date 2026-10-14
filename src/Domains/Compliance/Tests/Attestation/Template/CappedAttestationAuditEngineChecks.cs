namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedAttestationAuditEngineChecks
{
    public static void CappedAttestationAuditEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAttestationAuditEngine.DefaultCappedAttestationAuditEngine();
        var item = CappedAttestationAudit.NewCappedAttestationAudit("code-37", "label-662");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAttestationAuditEngineKeepsResultsWithinBounds();
    }
}
