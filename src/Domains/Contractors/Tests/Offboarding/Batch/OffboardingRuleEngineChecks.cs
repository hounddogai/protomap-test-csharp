namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OffboardingRuleEngineChecks
{
    public static void OffboardingRuleEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingRuleEngine.DefaultOffboardingRuleEngine(Flags.NewRegistry());
        var item = OffboardingRule.NewOffboardingRule("code-927", "label-508");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingRuleEngineKeepsResultsWithinBounds();
    }
}
