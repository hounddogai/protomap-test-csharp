namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingPolicyAckGroupViewChecks
{
    public static void PendingPolicyAckGroupViewShowsTheStatusInCapitals()
    {
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("code-154", "title-79");
        var view = PendingPolicyAckGroupView.ToPendingPolicyAckGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPolicyAckGroupView.ToPendingPolicyAckGroupViews(new List<PendingPolicyAckGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPolicyAckGroupViewShowsTheStatusInCapitals();
    }
}
