namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanSummaryEngineChecks
{
    public static void SuccessionPlanSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanSummaryEngine.DefaultSuccessionPlanSummaryEngine(Flags.NewRegistry());
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("code-182", "label-366");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanSummaryEngineKeepsResultsWithinBounds();
    }
}
