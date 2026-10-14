namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionManifestStoreChecks
{
    public static void MemoryExceptionManifestStoreFindsWhatItSaved()
    {
        var store = ExceptionManifestStore.NewMemoryExceptionManifestStore();
        var item = ExceptionManifest.NewExceptionManifest("code-333", "title-102");
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
        MemoryExceptionManifestStoreFindsWhatItSaved();
    }
}
