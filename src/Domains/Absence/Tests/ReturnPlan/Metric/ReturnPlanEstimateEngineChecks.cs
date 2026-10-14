namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanEstimateEngineChecks
{
    public static void ReturnPlanEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanEstimateEngine.DefaultReturnPlanEstimateEngine(Flags.NewRegistry());
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("code-672", "name-674");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanEstimateEngineKeepsResultsWithinBounds();
    }
}
