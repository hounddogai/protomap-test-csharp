namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BalanceAdjustmentEngineChecks
{
    public static void BalanceAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceAdjustmentEngine.DefaultBalanceAdjustmentEngine(Flags.NewRegistry());
        var item = BalanceAdjustment.NewBalanceAdjustment("code-491", "title-863");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceAdjustmentEngineKeepsResultsWithinBounds();
    }
}
