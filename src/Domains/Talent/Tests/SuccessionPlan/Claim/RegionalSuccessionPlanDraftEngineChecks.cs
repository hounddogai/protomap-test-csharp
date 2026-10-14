namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanDraftEngineChecks
{
    public static void RegionalSuccessionPlanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSuccessionPlanDraftEngine.DefaultRegionalSuccessionPlanDraftEngine(Flags.NewRegistry());
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("code-489", "title-653");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanDraftEngineKeepsResultsWithinBounds();
    }
}
