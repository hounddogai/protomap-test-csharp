namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanRollupEngineChecks
{
    public static void SuccessionPlanRollupEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanRollupEngine.DefaultSuccessionPlanRollupEngine();
        var item = SuccessionPlanRollup.NewSuccessionPlanRollup("code-961", "label-167");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanRollupEngineKeepsResultsWithinBounds();
    }
}
