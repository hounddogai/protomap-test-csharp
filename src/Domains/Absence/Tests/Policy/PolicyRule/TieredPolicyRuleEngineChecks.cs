namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class TieredPolicyRuleEngineChecks
{
    public static void TieredPolicyRuleEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPolicyRuleEngine.DefaultTieredPolicyRuleEngine();
        var item = TieredPolicyRule.NewTieredPolicyRule("code-596", "name-506");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPolicyRuleEngineKeepsResultsWithinBounds();
    }
}
