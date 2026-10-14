namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AuditTierChecks
{
    public static void AuditTierStartsInDraft()
    {
        var item = AuditTier.NewAuditTier("code-494", "title-434");
        Check.Equal(item.Status, AuditTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditTierMovesToSuspended()
    {
        var item = AuditTier.NewAuditTier("code-494", "title-434");
        try
        {
            item.MoveTo(AuditTierStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditTierStatus.Suspended, "status");
    }

    public static void AuditTierRejectsMovesFromClosed()
    {
        var item = AuditTier.NewAuditTier("code-494", "title-434");
        item.Status = AuditTierStatus.Closed;
        Check.Fails(() => item.MoveTo(AuditTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AuditTierStartsInDraft();
        AuditTierMovesToSuspended();
        AuditTierRejectsMovesFromClosed();
    }
}
