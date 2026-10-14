namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalAuditClaimChecks
{
    public static void SeasonalAuditClaimStartsInDraft()
    {
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("code-665", "label-755");
        Check.Equal(item.Status, SeasonalAuditClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAuditClaimMovesToSuspended()
    {
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("code-665", "label-755");
        try
        {
            item.MoveTo(SeasonalAuditClaimStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAuditClaimStatus.Suspended, "status");
    }

    public static void SeasonalAuditClaimRejectsMovesFromClosed()
    {
        var item = SeasonalAuditClaim.NewSeasonalAuditClaim("code-665", "label-755");
        item.Status = SeasonalAuditClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalAuditClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalAuditClaimStartsInDraft();
        SeasonalAuditClaimMovesToSuspended();
        SeasonalAuditClaimRejectsMovesFromClosed();
    }
}
