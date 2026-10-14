namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingPolicyAckMilestoneViewChecks
{
    public static void RollingPolicyAckMilestoneViewShowsTheStatusInCapitals()
    {
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("code-456", "title-540");
        var view = RollingPolicyAckMilestoneView.ToRollingPolicyAckMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPolicyAckMilestoneView.ToRollingPolicyAckMilestoneViews(new List<RollingPolicyAckMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPolicyAckMilestoneViewShowsTheStatusInCapitals();
    }
}
