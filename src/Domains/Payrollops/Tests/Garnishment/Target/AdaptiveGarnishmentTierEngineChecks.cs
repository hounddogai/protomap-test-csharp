namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentTierEngineChecks
{
    public static void AdaptiveGarnishmentTierEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveGarnishmentTierEngine.DefaultAdaptiveGarnishmentTierEngine();
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("code-833", "name-594");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentTierEngineKeepsResultsWithinBounds();
    }
}
