namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PendingExceptionProposalChecks
{
    public static void PendingExceptionProposalStartsInRequested()
    {
        var item = PendingExceptionProposal.NewPendingExceptionProposal("code-498", "title-702");
        Check.Equal(item.Status, PendingExceptionProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingExceptionProposalMovesToRevoked()
    {
        var item = PendingExceptionProposal.NewPendingExceptionProposal("code-498", "title-702");
        try
        {
            item.MoveTo(PendingExceptionProposalStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingExceptionProposalStatus.Revoked, "status");
    }

    public static void PendingExceptionProposalRejectsMovesFromExpired()
    {
        var item = PendingExceptionProposal.NewPendingExceptionProposal("code-498", "title-702");
        item.Status = PendingExceptionProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingExceptionProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingExceptionProposalStartsInRequested();
        PendingExceptionProposalMovesToRevoked();
        PendingExceptionProposalRejectsMovesFromExpired();
    }
}
