namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentPlanEngineChecks
{
    public static void AdaptiveGarnishmentPlanEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveGarnishmentPlanEngine.DefaultAdaptiveGarnishmentPlanEngine();
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("code-184", "title-607");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentPlanEngineKeepsResultsWithinBounds();
    }
}
