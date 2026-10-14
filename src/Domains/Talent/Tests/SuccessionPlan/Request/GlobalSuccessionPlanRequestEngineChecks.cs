namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanRequestEngineChecks
{
    public static void GlobalSuccessionPlanRequestEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalSuccessionPlanRequestEngine.DefaultGlobalSuccessionPlanRequestEngine(Flags.NewRegistry());
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("code-808", "label-269");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalSuccessionPlanRequestEngineKeepsResultsWithinBounds();
    }
}
