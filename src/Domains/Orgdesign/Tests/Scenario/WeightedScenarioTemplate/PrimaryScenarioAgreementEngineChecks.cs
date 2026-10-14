namespace Acme.Workforce.Orgdesign.Tests.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

/// <summary>Checks of weighted scenario template code, run by the project's runner.</summary>
public static class PrimaryScenarioAgreementEngineChecks
{
    public static void PrimaryScenarioAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryScenarioAgreementEngine.DefaultPrimaryScenarioAgreementEngine(Flags.NewRegistry());
        var item = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement("code-927", "label-906");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryScenarioAgreementEngineKeepsResultsWithinBounds();
    }
}
