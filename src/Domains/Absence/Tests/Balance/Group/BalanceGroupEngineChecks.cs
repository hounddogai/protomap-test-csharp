namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceGroupEngineChecks
{
    public static void BalanceGroupEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceGroupEngine.DefaultBalanceGroupEngine(Flags.NewRegistry());
        var item = BalanceGroup.NewBalanceGroup("code-233", "label-372");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceGroupEngineKeepsResultsWithinBounds();
    }
}
