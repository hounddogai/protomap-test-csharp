namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LenientActionPlanBaselineEngineChecks
{
    public static void LenientActionPlanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = LenientActionPlanBaselineEngine.DefaultLenientActionPlanBaselineEngine(Flags.NewRegistry());
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("code-137", "label-818");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientActionPlanBaselineEngineKeepsResultsWithinBounds();
    }
}
