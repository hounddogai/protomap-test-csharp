namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class FallbackPolicyAckPreferenceViewChecks
{
    public static void FallbackPolicyAckPreferenceViewShowsTheStatusInCapitals()
    {
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("code-192", "title-859");
        var view = FallbackPolicyAckPreferenceView.ToFallbackPolicyAckPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackPolicyAckPreferenceView.ToFallbackPolicyAckPreferenceViews(new List<FallbackPolicyAckPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackPolicyAckPreferenceViewShowsTheStatusInCapitals();
    }
}
