namespace Acme.Workforce.Compliance.Tests.Audit.AuditManifest;

using Acme.Workforce.Compliance.Audit.AuditManifest;

/// <summary>Checks of audit manifest code, run by the project's runner.</summary>
public static class LegacyAuditManifestStoreChecks
{
    public static void MemoryLegacyAuditManifestStoreFindsWhatItSaved()
    {
        var store = LegacyAuditManifestStore.NewMemoryLegacyAuditManifestStore();
        var item = LegacyAuditManifest.NewLegacyAuditManifest("code-674", "name-664");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryLegacyAuditManifestStoreFindsWhatItSaved();
    }
}
