namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanClaimEngineChecks
{
    public static void LenientSuccessionPlanClaimEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSuccessionPlanClaimEngine.DefaultLenientSuccessionPlanClaimEngine(Flags.NewRegistry());
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("code-597", "label-734");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanClaimEngineKeepsResultsWithinBounds();
    }
}
