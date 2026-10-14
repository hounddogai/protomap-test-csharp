namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackAuditManifestViewChecks
{
    public static void FallbackAuditManifestViewShowsTheStatusInCapitals()
    {
        var item = FallbackAuditManifest.NewFallbackAuditManifest("code-104", "label-729");
        var view = FallbackAuditManifestView.ToFallbackAuditManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAuditManifestView.ToFallbackAuditManifestViews(new List<FallbackAuditManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAuditManifestViewShowsTheStatusInCapitals();
    }
}
