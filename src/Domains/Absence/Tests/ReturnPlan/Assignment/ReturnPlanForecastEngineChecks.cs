namespace Acme.Workforce.Absence.Tests.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ReturnPlanForecastEngineChecks
{
    public static void ReturnPlanForecastEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanForecastEngine.DefaultReturnPlanForecastEngine();
        var item = ReturnPlanForecast.NewReturnPlanForecast("code-907", "title-653");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanForecastEngineKeepsResultsWithinBounds();
    }
}
