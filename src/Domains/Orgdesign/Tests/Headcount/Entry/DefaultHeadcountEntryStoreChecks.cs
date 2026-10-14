namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultHeadcountEntryStoreChecks
{
    public static void MemoryDefaultHeadcountEntryStoreFindsWhatItSaved()
    {
        var store = DefaultHeadcountEntryStore.NewMemoryDefaultHeadcountEntryStore();
        var item = DefaultHeadcountEntry.NewDefaultHeadcountEntry("code-98", "label-802");
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
        MemoryDefaultHeadcountEntryStoreFindsWhatItSaved();
    }
}
