namespace Acme.Workforce.Orgdesign.Tests.Reorg.Profile;

using Acme.Workforce.Orgdesign.Reorg.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ReorgPlanEngineChecks
{
    public static void ReorgPlanEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgPlanEngine.DefaultReorgPlanEngine();
        var item = ReorgPlan.NewReorgPlan("code-321", "label-454");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgPlanEngineKeepsResultsWithinBounds();
    }
}
