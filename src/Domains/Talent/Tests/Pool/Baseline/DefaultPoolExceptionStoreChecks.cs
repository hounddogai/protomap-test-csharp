namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolExceptionStoreChecks
{
    public static void MemoryDefaultPoolExceptionStoreFindsWhatItSaved()
    {
        var store = DefaultPoolExceptionStore.NewMemoryDefaultPoolExceptionStore();
        var item = DefaultPoolException.NewDefaultPoolException("code-237", "title-233");
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
        MemoryDefaultPoolExceptionStoreFindsWhatItSaved();
    }
}
