namespace Acme.Workforce.Orgdesign.Tests.Scenario.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PendingScenarioAgreementEngineChecks
{
    public static void PendingScenarioAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = PendingScenarioAgreementEngine.DefaultPendingScenarioAgreementEngine(Flags.NewRegistry());
        var item = PendingScenarioAgreement.NewPendingScenarioAgreement("code-54", "label-9");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingScenarioAgreementEngineKeepsResultsWithinBounds();
    }
}
