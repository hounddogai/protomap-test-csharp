namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictAuditManifestViewChecks
{
    public static void StrictAuditManifestViewShowsTheStatusInCapitals()
    {
        var item = StrictAuditManifest.NewStrictAuditManifest("code-68", "title-145");
        var view = StrictAuditManifestView.ToStrictAuditManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAuditManifestView.ToStrictAuditManifestViews(new List<StrictAuditManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAuditManifestViewShowsTheStatusInCapitals();
    }
}
