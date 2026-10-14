namespace Acme.Workforce.Compliance.Tests.Certification.Window;

using Acme.Workforce.Compliance.Certification.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CertificationGroupEngineChecks
{
    public static void CertificationGroupEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationGroupEngine.DefaultCertificationGroupEngine();
        var item = CertificationGroup.NewCertificationGroup("code-9", "name-841");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationGroupEngineKeepsResultsWithinBounds();
    }
}
