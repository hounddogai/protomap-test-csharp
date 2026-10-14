namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalWarrantyLedgerEngineChecks
{
    public static void RegionalWarrantyLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalWarrantyLedgerEngine.DefaultRegionalWarrantyLedgerEngine();
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("code-688", "title-360");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalWarrantyLedgerEngineKeepsResultsWithinBounds();
    }
}
