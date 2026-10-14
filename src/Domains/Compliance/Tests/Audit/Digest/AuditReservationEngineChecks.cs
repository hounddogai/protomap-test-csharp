namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditReservationEngineChecks
{
    public static void AuditReservationEngineKeepsResultsWithinBounds()
    {
        var engine = AuditReservationEngine.DefaultAuditReservationEngine(Flags.NewRegistry());
        var item = AuditReservation.NewAuditReservation("code-612", "title-227");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditReservationEngineKeepsResultsWithinBounds();
    }
}
