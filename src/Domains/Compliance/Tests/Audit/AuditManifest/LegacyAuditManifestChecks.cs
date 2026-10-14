namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class LegacyAuditManifestChecks
{
    public static void LegacyAuditManifestStartsInNew()
    {
        var item = LegacyAuditManifest.NewLegacyAuditManifest("code-674", "name-664");
        Check.Equal(item.Status, LegacyAuditManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAuditManifestMovesToQueued()
    {
        var item = LegacyAuditManifest.NewLegacyAuditManifest("code-674", "name-664");
        try
        {
            item.MoveTo(LegacyAuditManifestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAuditManifestStatus.Queued, "status");
    }

    public static void LegacyAuditManifestRejectsMovesFromFailed()
    {
        var item = LegacyAuditManifest.NewLegacyAuditManifest("code-674", "name-664");
        item.Status = LegacyAuditManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyAuditManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyAuditManifestStartsInNew();
        LegacyAuditManifestMovesToQueued();
        LegacyAuditManifestRejectsMovesFromFailed();
    }
}
