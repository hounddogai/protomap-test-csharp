namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalPolicyAckBundleChecks
{
    public static void GlobalPolicyAckBundleStartsInDraft()
    {
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("code-987", "title-677");
        Check.Equal(item.Status, GlobalPolicyAckBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPolicyAckBundleMovesToPending()
    {
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("code-987", "title-677");
        try
        {
            item.MoveTo(GlobalPolicyAckBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPolicyAckBundleStatus.Pending, "status");
    }

    public static void GlobalPolicyAckBundleRejectsMovesFromClosed()
    {
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("code-987", "title-677");
        item.Status = GlobalPolicyAckBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalPolicyAckBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalPolicyAckBundleStartsInDraft();
        GlobalPolicyAckBundleMovesToPending();
        GlobalPolicyAckBundleRejectsMovesFromClosed();
    }
}
