namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingPolicyAckMilestoneChecks
{
    public static void RollingPolicyAckMilestoneStartsInRequested()
    {
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("code-456", "title-540");
        Check.Equal(item.Status, RollingPolicyAckMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPolicyAckMilestoneMovesToExpired()
    {
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("code-456", "title-540");
        try
        {
            item.MoveTo(RollingPolicyAckMilestoneStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPolicyAckMilestoneStatus.Expired, "status");
    }

    public static void RollingPolicyAckMilestoneRejectsMovesFromExpired()
    {
        var item = RollingPolicyAckMilestone.NewRollingPolicyAckMilestone("code-456", "title-540");
        item.Status = RollingPolicyAckMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingPolicyAckMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingPolicyAckMilestoneStartsInRequested();
        RollingPolicyAckMilestoneMovesToExpired();
        RollingPolicyAckMilestoneRejectsMovesFromExpired();
    }
}
