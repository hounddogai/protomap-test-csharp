namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackAuditManifestStoreChecks
{
    public static void MemoryFallbackAuditManifestStoreFindsWhatItSaved()
    {
        var store = FallbackAuditManifestStore.NewMemoryFallbackAuditManifestStore();
        var item = FallbackAuditManifest.NewFallbackAuditManifest("code-104", "label-729");
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
        MemoryFallbackAuditManifestStoreFindsWhatItSaved();
    }
}
