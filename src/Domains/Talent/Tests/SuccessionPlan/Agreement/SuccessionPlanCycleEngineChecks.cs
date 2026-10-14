namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SuccessionPlanCycleEngineChecks
{
    public static void SuccessionPlanCycleEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanCycleEngine.DefaultSuccessionPlanCycleEngine(Flags.NewRegistry());
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("code-221", "label-18");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanCycleEngineKeepsResultsWithinBounds();
    }
}
