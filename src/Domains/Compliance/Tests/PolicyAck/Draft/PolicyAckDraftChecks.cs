namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckDraftChecks
{
    public static void PolicyAckDraftStartsInOpen()
    {
        var item = PolicyAckDraft.NewPolicyAckDraft("code-496", "title-705");
        Check.Equal(item.Status, PolicyAckDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckDraftMovesToReopened()
    {
        var item = PolicyAckDraft.NewPolicyAckDraft("code-496", "title-705");
        try
        {
            item.MoveTo(PolicyAckDraftStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckDraftStatus.Reopened, "status");
    }

    public static void PolicyAckDraftRejectsMovesFromArchived()
    {
        var item = PolicyAckDraft.NewPolicyAckDraft("code-496", "title-705");
        item.Status = PolicyAckDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(PolicyAckDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        PolicyAckDraftStartsInOpen();
        PolicyAckDraftMovesToReopened();
        PolicyAckDraftRejectsMovesFromArchived();
    }
}
