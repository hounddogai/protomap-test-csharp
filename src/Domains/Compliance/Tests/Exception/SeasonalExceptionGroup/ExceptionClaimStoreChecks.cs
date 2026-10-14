namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionClaimStoreChecks
{
    public static void MemoryExceptionClaimStoreFindsWhatItSaved()
    {
        var store = ExceptionClaimStore.NewMemoryExceptionClaimStore();
        var item = ExceptionClaim.NewExceptionClaim("code-868", "name-797");
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
        MemoryExceptionClaimStoreFindsWhatItSaved();
    }
}
