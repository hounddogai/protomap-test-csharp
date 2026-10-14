namespace Acme.Workforce.Compliance.Tests.Certification.CertificationSummary;

using Acme.Workforce.Compliance.Certification.CertificationSummary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of certification summary code, run by the project's runner.</summary>
public static class CertificationReservationEngineChecks
{
    public static void CertificationReservationEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationReservationEngine.DefaultCertificationReservationEngine(Flags.NewRegistry());
        var item = CertificationReservation.NewCertificationReservation("code-722", "label-127");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationReservationEngineKeepsResultsWithinBounds();
    }
}
