namespace Acme.Workforce.Compliance.Tests.Certification.Template;

using Acme.Workforce.Compliance.Certification.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CertificationPlanEngineChecks
{
    public static void CertificationPlanEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationPlanEngine.DefaultCertificationPlanEngine();
        var item = CertificationPlan.NewCertificationPlan("code-465", "name-549");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationPlanEngineKeepsResultsWithinBounds();
    }
}
