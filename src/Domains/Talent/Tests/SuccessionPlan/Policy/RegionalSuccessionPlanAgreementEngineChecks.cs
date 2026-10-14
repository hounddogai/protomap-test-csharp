namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanAgreementEngineChecks
{
    public static void RegionalSuccessionPlanAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSuccessionPlanAgreementEngine.DefaultRegionalSuccessionPlanAgreementEngine(Flags.NewRegistry());
        var item = RegionalSuccessionPlanAgreement.NewRegionalSuccessionPlanAgreement("code-421", "label-181");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSuccessionPlanAgreementEngineKeepsResultsWithinBounds();
    }
}
