namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class PolicyAckPreferenceChecks
{
    public static void PolicyAckPreferenceStartsInIdle()
    {
        var item = PolicyAckPreference.NewPolicyAckPreference("code-252", "name-253");
        Check.Equal(item.Status, PolicyAckPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckPreferenceMovesToWarming()
    {
        var item = PolicyAckPreference.NewPolicyAckPreference("code-252", "name-253");
        try
        {
            item.MoveTo(PolicyAckPreferenceStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckPreferenceStatus.Warming, "status");
    }

    public static void PolicyAckPreferenceRejectsMovesFromStopped()
    {
        var item = PolicyAckPreference.NewPolicyAckPreference("code-252", "name-253");
        item.Status = PolicyAckPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyAckPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyAckPreferenceStartsInIdle();
        PolicyAckPreferenceMovesToWarming();
        PolicyAckPreferenceRejectsMovesFromStopped();
    }
}
