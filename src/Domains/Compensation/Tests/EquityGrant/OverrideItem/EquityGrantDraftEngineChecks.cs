namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantDraftEngineChecks
{
    public static void EquityGrantDraftEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantDraftEngine.DefaultEquityGrantDraftEngine();
        var item = EquityGrantDraft.NewEquityGrantDraft("code-349", "title-843");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantDraftEngineKeepsResultsWithinBounds();
    }
}
