namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class PolicyMilestoneEngineChecks
{
    public static void PolicyMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyMilestoneEngine.DefaultPolicyMilestoneEngine();
        var item = PolicyMilestone.NewPolicyMilestone("code-753", "title-971");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyMilestoneEngineKeepsResultsWithinBounds();
    }
}
