namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceBatchEngineChecks
{
    public static void LenientBalanceBatchEngineKeepsResultsWithinBounds()
    {
        var engine = LenientBalanceBatchEngine.DefaultLenientBalanceBatchEngine(Flags.NewRegistry());
        var item = LenientBalanceBatch.NewLenientBalanceBatch("code-703", "title-981");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientBalanceBatchEngineKeepsResultsWithinBounds();
    }
}
