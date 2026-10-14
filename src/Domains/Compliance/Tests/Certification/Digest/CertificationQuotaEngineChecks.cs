namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CertificationQuotaEngineChecks
{
    public static void CertificationQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationQuotaEngine.DefaultCertificationQuotaEngine();
        var item = CertificationQuota.NewCertificationQuota("code-204", "title-251");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationQuotaEngineKeepsResultsWithinBounds();
    }
}
