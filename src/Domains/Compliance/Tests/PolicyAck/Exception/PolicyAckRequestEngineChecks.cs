namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckRequestEngineChecks
{
    public static void PolicyAckRequestEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckRequestEngine.DefaultPolicyAckRequestEngine(Flags.NewRegistry());
        var item = PolicyAckRequest.NewPolicyAckRequest("code-606", "title-716");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckRequestEngineKeepsResultsWithinBounds();
    }
}
