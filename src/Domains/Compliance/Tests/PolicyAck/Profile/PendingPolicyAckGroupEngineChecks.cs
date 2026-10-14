namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingPolicyAckGroupEngineChecks
{
    public static void PendingPolicyAckGroupEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPolicyAckGroupEngine.DefaultPendingPolicyAckGroupEngine(Flags.NewRegistry());
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("code-154", "title-79");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPolicyAckGroupEngineKeepsResultsWithinBounds();
    }
}
