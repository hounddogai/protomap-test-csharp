namespace Acme.Workforce.Compliance.Tests.PolicyAck.Bundle;

using Acme.Workforce.Compliance.PolicyAck.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PolicyAckBundleChecks
{
    public static void PolicyAckBundleStartsInProposed()
    {
        var item = PolicyAckBundle.NewPolicyAckBundle("code-135", "name-803");
        Check.Equal(item.Status, PolicyAckBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckBundleMovesToRejected()
    {
        var item = PolicyAckBundle.NewPolicyAckBundle("code-135", "name-803");
        try
        {
            item.MoveTo(PolicyAckBundleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckBundleStatus.Rejected, "status");
    }

    public static void PolicyAckBundleRejectsMovesFromWithdrawn()
    {
        var item = PolicyAckBundle.NewPolicyAckBundle("code-135", "name-803");
        item.Status = PolicyAckBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyAckBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyAckBundleStartsInProposed();
        PolicyAckBundleMovesToRejected();
        PolicyAckBundleRejectsMovesFromWithdrawn();
    }
}
