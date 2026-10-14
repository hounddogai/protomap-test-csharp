namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyRuleEngineChecks
{
    public static void PolicyRuleEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyRuleEngine.DefaultPolicyRuleEngine();
        var item = PolicyRule.NewPolicyRule("code-765", "name-162");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyRuleEngineKeepsResultsWithinBounds();
    }
}
