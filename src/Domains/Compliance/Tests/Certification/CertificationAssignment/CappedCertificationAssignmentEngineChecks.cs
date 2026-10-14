namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class CappedCertificationAssignmentEngineChecks
{
    public static void CappedCertificationAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedCertificationAssignmentEngine.DefaultCappedCertificationAssignmentEngine(Flags.NewRegistry());
        var item = CappedCertificationAssignment.NewCappedCertificationAssignment("code-392", "title-360");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedCertificationAssignmentEngineKeepsResultsWithinBounds();
    }
}
