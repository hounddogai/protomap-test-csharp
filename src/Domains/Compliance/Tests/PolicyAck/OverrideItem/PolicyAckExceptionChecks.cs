namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckExceptionChecks
{
    public static void PolicyAckExceptionStartsInProposed()
    {
        var item = PolicyAckException.NewPolicyAckException("code-720", "label-978");
        Check.Equal(item.Status, PolicyAckExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckExceptionMovesToApproved()
    {
        var item = PolicyAckException.NewPolicyAckException("code-720", "label-978");
        try
        {
            item.MoveTo(PolicyAckExceptionStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckExceptionStatus.Approved, "status");
    }

    public static void PolicyAckExceptionRejectsMovesFromWithdrawn()
    {
        var item = PolicyAckException.NewPolicyAckException("code-720", "label-978");
        item.Status = PolicyAckExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyAckExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyAckExceptionStartsInProposed();
        PolicyAckExceptionMovesToApproved();
        PolicyAckExceptionRejectsMovesFromWithdrawn();
    }
}
