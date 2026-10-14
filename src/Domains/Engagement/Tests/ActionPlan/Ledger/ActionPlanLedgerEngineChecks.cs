namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ActionPlanLedgerEngineChecks
{
    public static void ActionPlanLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = ActionPlanLedgerEngine.DefaultActionPlanLedgerEngine(Flags.NewRegistry());
        var item = ActionPlanLedger.NewActionPlanLedger("code-982", "title-662");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ActionPlanLedgerEngineKeepsResultsWithinBounds();
    }
}
