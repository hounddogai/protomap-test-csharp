namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckOverrideChecks
{
    public static void PolicyAckOverrideStartsInDraft()
    {
        var item = PolicyAckOverride.NewPolicyAckOverride("code-575", "label-308");
        Check.Equal(item.Status, PolicyAckOverrideStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckOverrideMovesToPending()
    {
        var item = PolicyAckOverride.NewPolicyAckOverride("code-575", "label-308");
        try
        {
            item.MoveTo(PolicyAckOverrideStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckOverrideStatus.Pending, "status");
    }

    public static void PolicyAckOverrideRejectsMovesFromClosed()
    {
        var item = PolicyAckOverride.NewPolicyAckOverride("code-575", "label-308");
        item.Status = PolicyAckOverrideStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyAckOverrideStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyAckOverrideStartsInDraft();
        PolicyAckOverrideMovesToPending();
        PolicyAckOverrideRejectsMovesFromClosed();
    }
}
