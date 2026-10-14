namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckNoticeChecks
{
    public static void PolicyAckNoticeStartsInDraft()
    {
        var item = PolicyAckNotice.NewPolicyAckNotice("code-525", "title-596");
        Check.Equal(item.Status, PolicyAckNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckNoticeMovesToPending()
    {
        var item = PolicyAckNotice.NewPolicyAckNotice("code-525", "title-596");
        try
        {
            item.MoveTo(PolicyAckNoticeStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckNoticeStatus.Pending, "status");
    }

    public static void PolicyAckNoticeRejectsMovesFromClosed()
    {
        var item = PolicyAckNotice.NewPolicyAckNotice("code-525", "title-596");
        item.Status = PolicyAckNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyAckNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyAckNoticeStartsInDraft();
        PolicyAckNoticeMovesToPending();
        PolicyAckNoticeRejectsMovesFromClosed();
    }
}
