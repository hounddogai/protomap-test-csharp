namespace Acme.Workforce.Compliance.Tests.Exception.Proposal;

using Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ExceptionOverrideStoreChecks
{
    public static void MemoryExceptionOverrideStoreFindsWhatItSaved()
    {
        var store = ExceptionOverrideStore.NewMemoryExceptionOverrideStore();
        var item = ExceptionOverride.NewExceptionOverride("code-161", "name-834");
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
        MemoryExceptionOverrideStoreFindsWhatItSaved();
    }
}
