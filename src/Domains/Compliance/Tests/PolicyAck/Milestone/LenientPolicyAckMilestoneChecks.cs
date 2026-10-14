namespace Acme.Workforce.Compliance.Tests.PolicyAck.Milestone;

using Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LenientPolicyAckMilestoneChecks
{
    public static void LenientPolicyAckMilestoneStartsInDraft()
    {
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("code-118", "label-282");
        Check.Equal(item.Status, LenientPolicyAckMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPolicyAckMilestoneMovesToPending()
    {
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("code-118", "label-282");
        try
        {
            item.MoveTo(LenientPolicyAckMilestoneStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPolicyAckMilestoneStatus.Pending, "status");
    }

    public static void LenientPolicyAckMilestoneRejectsMovesFromClosed()
    {
        var item = LenientPolicyAckMilestone.NewLenientPolicyAckMilestone("code-118", "label-282");
        item.Status = LenientPolicyAckMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientPolicyAckMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientPolicyAckMilestoneStartsInDraft();
        LenientPolicyAckMilestoneMovesToPending();
        LenientPolicyAckMilestoneRejectsMovesFromClosed();
    }
}
