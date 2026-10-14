namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptivePositionPlanEngineChecks
{
    public static void AdaptivePositionPlanEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePositionPlanEngine.DefaultAdaptivePositionPlanEngine(Flags.NewRegistry());
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("code-422", "name-343");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePositionPlanEngineKeepsResultsWithinBounds();
    }
}
