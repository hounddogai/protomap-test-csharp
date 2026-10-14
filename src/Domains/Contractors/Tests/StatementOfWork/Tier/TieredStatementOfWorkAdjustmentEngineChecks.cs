namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredStatementOfWorkAdjustmentEngineChecks
{
    public static void TieredStatementOfWorkAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = TieredStatementOfWorkAdjustmentEngine.DefaultTieredStatementOfWorkAdjustmentEngine();
        var item = TieredStatementOfWorkAdjustment.NewTieredStatementOfWorkAdjustment("code-219", "label-311");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkAdjustmentEngineKeepsResultsWithinBounds();
    }
}
