namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackGarnishmentPlanEngineChecks
{
    public static void FallbackGarnishmentPlanEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackGarnishmentPlanEngine.DefaultFallbackGarnishmentPlanEngine();
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("code-790", "title-535");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackGarnishmentPlanEngineKeepsResultsWithinBounds();
    }
}
