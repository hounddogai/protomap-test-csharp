namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyAckItemChecks
{
    public static void PolicyAckItemStartsInRequested()
    {
        var item = PolicyAckItem.NewPolicyAckItem("code-191", "label-695");
        Check.Equal(item.Status, PolicyAckItemStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckItemMovesToGranted()
    {
        var item = PolicyAckItem.NewPolicyAckItem("code-191", "label-695");
        try
        {
            item.MoveTo(PolicyAckItemStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckItemStatus.Granted, "status");
    }

    public static void PolicyAckItemRejectsMovesFromExpired()
    {
        var item = PolicyAckItem.NewPolicyAckItem("code-191", "label-695");
        item.Status = PolicyAckItemStatus.Expired;
        Check.Fails(() => item.MoveTo(PolicyAckItemStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PolicyAckItemStartsInRequested();
        PolicyAckItemMovesToGranted();
        PolicyAckItemRejectsMovesFromExpired();
    }
}
