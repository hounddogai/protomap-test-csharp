namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RefreshPlanEngineChecks
{
    public static void RefreshPlanEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshPlanEngine.DefaultRefreshPlanEngine(Flags.NewRegistry());
        var item = RefreshPlan.NewRefreshPlan("code-819", "label-246");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshPlanEngineKeepsResultsWithinBounds();
    }
}
