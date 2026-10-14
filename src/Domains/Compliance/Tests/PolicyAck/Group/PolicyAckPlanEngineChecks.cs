namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckPlanEngineChecks
{
    public static void PolicyAckPlanEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckPlanEngine.DefaultPolicyAckPlanEngine();
        var item = PolicyAckPlan.NewPolicyAckPlan("code-66", "title-970");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckPlanEngineKeepsResultsWithinBounds();
    }
}
