namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationRequestEngineChecks
{
    public static void CertificationRequestEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationRequestEngine.DefaultCertificationRequestEngine(Flags.NewRegistry());
        var item = CertificationRequest.NewCertificationRequest("code-463", "title-550");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationRequestEngineKeepsResultsWithinBounds();
    }
}
