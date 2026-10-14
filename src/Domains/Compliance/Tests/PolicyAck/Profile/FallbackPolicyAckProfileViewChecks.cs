namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class FallbackPolicyAckProfileViewChecks
{
    public static void FallbackPolicyAckProfileViewShowsTheStatusInCapitals()
    {
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("code-388", "name-346");
        var view = FallbackPolicyAckProfileView.ToFallbackPolicyAckProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPolicyAckProfileView.ToFallbackPolicyAckProfileViews(new List<FallbackPolicyAckProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPolicyAckProfileViewShowsTheStatusInCapitals();
    }
}
