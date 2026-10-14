namespace Acme.Workforce.Assets.Tests.LicenseSeat.Cycle;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientLicenseSeatAuditEngineChecks
{
    public static void LenientLicenseSeatAuditEngineKeepsResultsWithinBounds()
    {
        var engine = LenientLicenseSeatAuditEngine.DefaultLenientLicenseSeatAuditEngine();
        var item = LenientLicenseSeatAudit.NewLenientLicenseSeatAudit("code-116", "title-916");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientLicenseSeatAuditEngineKeepsResultsWithinBounds();
    }
}
