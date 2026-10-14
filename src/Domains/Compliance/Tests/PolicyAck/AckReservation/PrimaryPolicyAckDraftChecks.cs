namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class PrimaryPolicyAckDraftChecks
{
    public static void PrimaryPolicyAckDraftStartsInOpen()
    {
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("code-778", "title-102");
        Check.Equal(item.Status, PrimaryPolicyAckDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPolicyAckDraftMovesToReopened()
    {
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("code-778", "title-102");
        try
        {
            item.MoveTo(PrimaryPolicyAckDraftStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPolicyAckDraftStatus.Reopened, "status");
    }

    public static void PrimaryPolicyAckDraftRejectsMovesFromArchived()
    {
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("code-778", "title-102");
        item.Status = PrimaryPolicyAckDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryPolicyAckDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckDraftStartsInOpen();
        PrimaryPolicyAckDraftMovesToReopened();
        PrimaryPolicyAckDraftRejectsMovesFromArchived();
    }
}
