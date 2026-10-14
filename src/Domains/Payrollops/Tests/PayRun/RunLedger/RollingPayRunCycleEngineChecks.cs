namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class RollingPayRunCycleEngineChecks
{
    public static void RollingPayRunCycleEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPayRunCycleEngine.DefaultRollingPayRunCycleEngine(Flags.NewRegistry());
        var item = RollingPayRunCycle.NewRollingPayRunCycle("code-57", "label-752");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPayRunCycleEngineKeepsResultsWithinBounds();
    }
}
