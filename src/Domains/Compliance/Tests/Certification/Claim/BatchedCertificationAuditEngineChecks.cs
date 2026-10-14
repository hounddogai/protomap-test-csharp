namespace Acme.Workforce.Compliance.Tests.Certification.Claim;

using Acme.Workforce.Compliance.Certification.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedCertificationAuditEngineChecks
{
    public static void BatchedCertificationAuditEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedCertificationAuditEngine.DefaultBatchedCertificationAuditEngine();
        var item = BatchedCertificationAudit.NewBatchedCertificationAudit("code-972", "name-418");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedCertificationAuditEngineKeepsResultsWithinBounds();
    }
}
