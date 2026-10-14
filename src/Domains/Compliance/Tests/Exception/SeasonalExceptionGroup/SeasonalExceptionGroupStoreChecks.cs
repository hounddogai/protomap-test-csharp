namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class SeasonalExceptionGroupStoreChecks
{
    public static void MemorySeasonalExceptionGroupStoreFindsWhatItSaved()
    {
        var store = SeasonalExceptionGroupStore.NewMemorySeasonalExceptionGroupStore();
        var item = SeasonalExceptionGroup.NewSeasonalExceptionGroup("code-16", "label-475");
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
        MemorySeasonalExceptionGroupStoreFindsWhatItSaved();
    }
}
