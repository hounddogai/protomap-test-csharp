namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackPolicyRuleEngineChecks
{
    public static void FallbackPolicyRuleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPolicyRuleEngine.DefaultFallbackPolicyRuleEngine();
        var item = FallbackPolicyRule.NewFallbackPolicyRule("code-483", "title-489");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPolicyRuleEngineKeepsResultsWithinBounds();
    }
}
