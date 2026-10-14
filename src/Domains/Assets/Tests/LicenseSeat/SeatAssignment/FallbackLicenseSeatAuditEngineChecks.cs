namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class FallbackLicenseSeatAuditEngineChecks
{
    public static void FallbackLicenseSeatAuditEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackLicenseSeatAuditEngine.DefaultFallbackLicenseSeatAuditEngine();
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("code-313", "name-298");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatAuditEngineKeepsResultsWithinBounds();
    }
}
