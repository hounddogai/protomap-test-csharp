namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionDigestStoreChecks
{
    public static void MemoryExceptionDigestStoreFindsWhatItSaved()
    {
        var store = ExceptionDigestStore.NewMemoryExceptionDigestStore();
        var item = ExceptionDigest.NewExceptionDigest("code-412", "name-333");
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
        MemoryExceptionDigestStoreFindsWhatItSaved();
    }
}
