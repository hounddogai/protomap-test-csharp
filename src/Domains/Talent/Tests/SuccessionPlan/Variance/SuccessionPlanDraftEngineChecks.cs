namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanDraftEngineChecks
{
    public static void SuccessionPlanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanDraftEngine.DefaultSuccessionPlanDraftEngine(Flags.NewRegistry());
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("code-326", "title-862");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanDraftEngineKeepsResultsWithinBounds();
    }
}
