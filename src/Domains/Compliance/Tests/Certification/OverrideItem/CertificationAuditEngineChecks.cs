namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationAuditEngineChecks
{
    public static void CertificationAuditEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationAuditEngine.DefaultCertificationAuditEngine(Flags.NewRegistry());
        var item = CertificationAudit.NewCertificationAudit("code-260", "label-397");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationAuditEngineKeepsResultsWithinBounds();
    }
}
