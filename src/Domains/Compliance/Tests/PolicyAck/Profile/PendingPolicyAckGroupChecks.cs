namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingPolicyAckGroupChecks
{
    public static void PendingPolicyAckGroupStartsInNew()
    {
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("code-154", "title-79");
        Check.Equal(item.Status, PendingPolicyAckGroupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPolicyAckGroupMovesToQueued()
    {
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("code-154", "title-79");
        try
        {
            item.MoveTo(PendingPolicyAckGroupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPolicyAckGroupStatus.Queued, "status");
    }

    public static void PendingPolicyAckGroupRejectsMovesFromFailed()
    {
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("code-154", "title-79");
        item.Status = PendingPolicyAckGroupStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingPolicyAckGroupStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingPolicyAckGroupStartsInNew();
        PendingPolicyAckGroupMovesToQueued();
        PendingPolicyAckGroupRejectsMovesFromFailed();
    }
}
