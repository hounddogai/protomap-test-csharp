namespace Acme.Workforce.Compliance.Tests.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LenientPolicyAckMilestoneEngineChecks
{
    public static void LenientPolicyAckMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPolicyAckMilestoneEngine.DefaultLenientPolicyAckMilestoneEngine(Flags.NewRegistry());
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("code-118", "label-282");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPolicyAckMilestoneEngineKeepsResultsWithinBounds();
    }
}
