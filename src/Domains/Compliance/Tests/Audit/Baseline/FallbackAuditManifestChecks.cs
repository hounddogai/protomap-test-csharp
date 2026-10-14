namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackAuditManifestChecks
{
    public static void FallbackAuditManifestStartsInRequested()
    {
        var item = FallbackAuditManifest.NewFallbackAuditManifest("code-104", "label-729");
        Check.Equal(item.Status, FallbackAuditManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAuditManifestMovesToRevoked()
    {
        var item = FallbackAuditManifest.NewFallbackAuditManifest("code-104", "label-729");
        try
        {
            item.MoveTo(FallbackAuditManifestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAuditManifestStatus.Revoked, "status");
    }

    public static void FallbackAuditManifestRejectsMovesFromExpired()
    {
        var item = FallbackAuditManifest.NewFallbackAuditManifest("code-104", "label-729");
        item.Status = FallbackAuditManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackAuditManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackAuditManifestStartsInRequested();
        FallbackAuditManifestMovesToRevoked();
        FallbackAuditManifestRejectsMovesFromExpired();
    }
}
