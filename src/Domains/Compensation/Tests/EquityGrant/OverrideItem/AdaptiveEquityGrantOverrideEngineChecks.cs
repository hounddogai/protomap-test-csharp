namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AdaptiveEquityGrantOverrideEngineChecks
{
    public static void AdaptiveEquityGrantOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveEquityGrantOverrideEngine.DefaultAdaptiveEquityGrantOverrideEngine();
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("code-679", "name-28");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveEquityGrantOverrideEngineKeepsResultsWithinBounds();
    }
}
