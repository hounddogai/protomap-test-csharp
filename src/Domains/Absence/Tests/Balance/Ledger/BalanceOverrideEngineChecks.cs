namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceOverrideEngineChecks
{
    public static void BalanceOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceOverrideEngine.DefaultBalanceOverrideEngine(Flags.NewRegistry());
        var item = BalanceOverride.NewBalanceOverride("code-757", "name-939");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceOverrideEngineKeepsResultsWithinBounds();
    }
}
