namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class LegacyAuditManifestViewChecks
{
    public static void LegacyAuditManifestViewShowsTheStatusInCapitals()
    {
        var item = LegacyAuditManifest.NewLegacyAuditManifest("code-674", "name-664");
        var view = LegacyAuditManifestView.ToLegacyAuditManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyAuditManifestView.ToLegacyAuditManifestViews(new List<LegacyAuditManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyAuditManifestViewShowsTheStatusInCapitals();
    }
}
