namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CostModelLedgerEngineChecks
{
    public static void CostModelLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelLedgerEngine.DefaultCostModelLedgerEngine();
        var item = CostModelLedger.NewCostModelLedger("code-841", "label-66");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelLedgerEngineKeepsResultsWithinBounds();
    }
}
