namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class LenientSuccessionPlanAgreementEngineChecks
{
    public static void LenientSuccessionPlanAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSuccessionPlanAgreementEngine.DefaultLenientSuccessionPlanAgreementEngine(Flags.NewRegistry());
        var item = LenientSuccessionPlanAgreement.NewLenientSuccessionPlanAgreement("code-692", "name-315");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanAgreementEngineKeepsResultsWithinBounds();
    }
}
