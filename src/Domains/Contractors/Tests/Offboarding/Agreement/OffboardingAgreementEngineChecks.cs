namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OffboardingAgreementEngineChecks
{
    public static void OffboardingAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingAgreementEngine.DefaultOffboardingAgreementEngine(Flags.NewRegistry());
        var item = OffboardingAgreement.NewOffboardingAgreement("code-819", "label-714");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingAgreementEngineKeepsResultsWithinBounds();
    }
}
