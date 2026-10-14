namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LenientScenarioAgreementEngineChecks
{
    public static void LenientScenarioAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LenientScenarioAgreementEngine.DefaultLenientScenarioAgreementEngine(Flags.NewRegistry());
        var item = LenientScenarioAgreement.NewLenientScenarioAgreement("code-47", "label-400");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientScenarioAgreementEngineKeepsResultsWithinBounds();
    }
}
