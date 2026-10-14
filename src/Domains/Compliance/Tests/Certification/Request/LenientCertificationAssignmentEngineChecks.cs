namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientCertificationAssignmentEngineChecks
{
    public static void LenientCertificationAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = LenientCertificationAssignmentEngine.DefaultLenientCertificationAssignmentEngine();
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("code-515", "title-388");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientCertificationAssignmentEngineKeepsResultsWithinBounds();
    }
}
