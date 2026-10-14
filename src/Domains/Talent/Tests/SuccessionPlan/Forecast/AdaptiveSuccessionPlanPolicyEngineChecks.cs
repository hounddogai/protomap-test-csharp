namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class AdaptiveSuccessionPlanPolicyEngineChecks
{
    public static void AdaptiveSuccessionPlanPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveSuccessionPlanPolicyEngine.DefaultAdaptiveSuccessionPlanPolicyEngine(Flags.NewRegistry());
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("code-832", "name-493");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveSuccessionPlanPolicyEngineKeepsResultsWithinBounds();
    }
}
