namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class CappedAuditReservationEngineChecks
{
    public static void CappedAuditReservationEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAuditReservationEngine.DefaultCappedAuditReservationEngine(Flags.NewRegistry());
        var item = CappedAuditReservation.NewCappedAuditReservation("code-212", "name-839");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAuditReservationEngineKeepsResultsWithinBounds();
    }
}
