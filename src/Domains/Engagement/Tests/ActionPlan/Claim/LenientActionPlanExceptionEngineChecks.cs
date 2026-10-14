namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientActionPlanExceptionEngineChecks
{
    public static void LenientActionPlanExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = LenientActionPlanExceptionEngine.DefaultLenientActionPlanExceptionEngine(Flags.NewRegistry());
        var item = LenientActionPlanException.NewLenientActionPlanException("code-952", "label-870");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientActionPlanExceptionEngineKeepsResultsWithinBounds();
    }
}
