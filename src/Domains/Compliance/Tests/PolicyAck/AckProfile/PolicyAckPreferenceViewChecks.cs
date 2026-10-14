namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class PolicyAckPreferenceViewChecks
{
    public static void PolicyAckPreferenceViewShowsTheStatusInCapitals()
    {
        var item = PolicyAckPreference.NewPolicyAckPreference("code-252", "name-253");
        var view = PolicyAckPreferenceView.ToPolicyAckPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAckPreferenceView.ToPolicyAckPreferenceViews(new List<PolicyAckPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAckPreferenceViewShowsTheStatusInCapitals();
    }
}
