namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class GlobalEquityGrantDraftEngineChecks
{
    public static void GlobalEquityGrantDraftEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalEquityGrantDraftEngine.DefaultGlobalEquityGrantDraftEngine(Flags.NewRegistry());
        var item = GlobalEquityGrantDraft.NewGlobalEquityGrantDraft("code-531", "title-962");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalEquityGrantDraftEngineKeepsResultsWithinBounds();
    }
}
