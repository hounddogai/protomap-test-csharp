namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PolicyAckBaselineChecks
{
    public static void PolicyAckBaselineStartsInDraft()
    {
        var item = PolicyAckBaseline.NewPolicyAckBaseline("code-343", "title-148");
        Check.Equal(item.Status, PolicyAckBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckBaselineMovesToPending()
    {
        var item = PolicyAckBaseline.NewPolicyAckBaseline("code-343", "title-148");
        try
        {
            item.MoveTo(PolicyAckBaselineStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckBaselineStatus.Pending, "status");
    }

    public static void PolicyAckBaselineRejectsMovesFromClosed()
    {
        var item = PolicyAckBaseline.NewPolicyAckBaseline("code-343", "title-148");
        item.Status = PolicyAckBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyAckBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyAckBaselineStartsInDraft();
        PolicyAckBaselineMovesToPending();
        PolicyAckBaselineRejectsMovesFromClosed();
    }
}
