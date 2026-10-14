namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyBalanceAdjustmentEngineChecks
{
    public static void LegacyBalanceAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyBalanceAdjustmentEngine.DefaultLegacyBalanceAdjustmentEngine(Flags.NewRegistry());
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("code-972", "title-629");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyBalanceAdjustmentEngineKeepsResultsWithinBounds();
    }
}
