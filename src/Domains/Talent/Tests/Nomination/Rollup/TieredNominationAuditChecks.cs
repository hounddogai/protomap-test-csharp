namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredNominationAuditChecks
{
    public static void TieredNominationAuditStartsInProposed()
    {
        var item = TieredNominationAudit.NewTieredNominationAudit("code-874", "title-997");
        Check.Equal(item.Status, TieredNominationAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredNominationAuditMovesToApproved()
    {
        var item = TieredNominationAudit.NewTieredNominationAudit("code-874", "title-997");
        try
        {
            item.MoveTo(TieredNominationAuditStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredNominationAuditStatus.Approved, "status");
    }

    public static void TieredNominationAuditRejectsMovesFromWithdrawn()
    {
        var item = TieredNominationAudit.NewTieredNominationAudit("code-874", "title-997");
        item.Status = TieredNominationAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredNominationAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredNominationAuditStartsInProposed();
        TieredNominationAuditMovesToApproved();
        TieredNominationAuditRejectsMovesFromWithdrawn();
    }
}
