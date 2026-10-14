namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationAuditChecks
{
    public static void NominationAuditStartsInProposed()
    {
        var item = NominationAudit.NewNominationAudit("code-156", "name-536");
        Check.Equal(item.Status, NominationAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationAuditMovesToRejected()
    {
        var item = NominationAudit.NewNominationAudit("code-156", "name-536");
        try
        {
            item.MoveTo(NominationAuditStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationAuditStatus.Rejected, "status");
    }

    public static void NominationAuditRejectsMovesFromWithdrawn()
    {
        var item = NominationAudit.NewNominationAudit("code-156", "name-536");
        item.Status = NominationAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(NominationAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        NominationAuditStartsInProposed();
        NominationAuditMovesToRejected();
        NominationAuditRejectsMovesFromWithdrawn();
    }
}
