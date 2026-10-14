namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class DefaultExceptionDigestStoreChecks
{
    public static void MemoryDefaultExceptionDigestStoreFindsWhatItSaved()
    {
        var store = DefaultExceptionDigestStore.NewMemoryDefaultExceptionDigestStore();
        var item = DefaultExceptionDigest.NewDefaultExceptionDigest("code-533", "label-981");
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
        MemoryDefaultExceptionDigestStoreFindsWhatItSaved();
    }
}
