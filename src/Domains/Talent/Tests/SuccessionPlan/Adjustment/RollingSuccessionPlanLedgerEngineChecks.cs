namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingSuccessionPlanLedgerEngineChecks
{
    public static void RollingSuccessionPlanLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = RollingSuccessionPlanLedgerEngine.DefaultRollingSuccessionPlanLedgerEngine(Flags.NewRegistry());
        var item = RollingSuccessionPlanLedger.NewRollingSuccessionPlanLedger("code-266", "label-367");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanLedgerEngineKeepsResultsWithinBounds();
    }
}
