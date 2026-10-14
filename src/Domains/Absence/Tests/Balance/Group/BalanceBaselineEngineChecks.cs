namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceBaselineEngineChecks
{
    public static void BalanceBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceBaselineEngine.DefaultBalanceBaselineEngine(Flags.NewRegistry());
        var item = BalanceBaseline.NewBalanceBaseline("code-445", "title-479");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceBaselineEngineKeepsResultsWithinBounds();
    }
}
