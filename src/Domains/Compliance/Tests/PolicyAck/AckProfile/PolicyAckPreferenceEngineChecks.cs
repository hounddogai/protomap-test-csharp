namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class PolicyAckPreferenceEngineChecks
{
    public static void PolicyAckPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckPreferenceEngine.DefaultPolicyAckPreferenceEngine();
        var item = PolicyAckPreference.NewPolicyAckPreference("code-252", "name-253");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckPreferenceEngineKeepsResultsWithinBounds();
    }
}
