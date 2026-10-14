namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictAuditManifestChecks
{
    public static void StrictAuditManifestStartsInRequested()
    {
        var item = StrictAuditManifest.NewStrictAuditManifest("code-68", "title-145");
        Check.Equal(item.Status, StrictAuditManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAuditManifestMovesToRevoked()
    {
        var item = StrictAuditManifest.NewStrictAuditManifest("code-68", "title-145");
        try
        {
            item.MoveTo(StrictAuditManifestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAuditManifestStatus.Revoked, "status");
    }

    public static void StrictAuditManifestRejectsMovesFromExpired()
    {
        var item = StrictAuditManifest.NewStrictAuditManifest("code-68", "title-145");
        item.Status = StrictAuditManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictAuditManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictAuditManifestStartsInRequested();
        StrictAuditManifestMovesToRevoked();
        StrictAuditManifestRejectsMovesFromExpired();
    }
}
