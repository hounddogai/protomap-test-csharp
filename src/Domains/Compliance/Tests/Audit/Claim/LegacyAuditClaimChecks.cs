namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LegacyAuditClaimChecks
{
    public static void LegacyAuditClaimStartsInProposed()
    {
        var item = LegacyAuditClaim.NewLegacyAuditClaim("code-931", "title-362");
        Check.Equal(item.Status, LegacyAuditClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAuditClaimMovesToApproved()
    {
        var item = LegacyAuditClaim.NewLegacyAuditClaim("code-931", "title-362");
        try
        {
            item.MoveTo(LegacyAuditClaimStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAuditClaimStatus.Approved, "status");
    }

    public static void LegacyAuditClaimRejectsMovesFromWithdrawn()
    {
        var item = LegacyAuditClaim.NewLegacyAuditClaim("code-931", "title-362");
        item.Status = LegacyAuditClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyAuditClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyAuditClaimStartsInProposed();
        LegacyAuditClaimMovesToApproved();
        LegacyAuditClaimRejectsMovesFromWithdrawn();
    }
}
