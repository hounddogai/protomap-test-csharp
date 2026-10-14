namespace Acme.Workforce.Compliance.Tests.Audit.Cycle;

using Acme.Workforce.Compliance.Audit.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedAuditLedgerEngineChecks
{
    public static void WeightedAuditLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAuditLedgerEngine.DefaultWeightedAuditLedgerEngine();
        var item = WeightedAuditLedger.NewWeightedAuditLedger("code-732", "label-640");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAuditLedgerEngineKeepsResultsWithinBounds();
    }
}
