namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultHeadcountProfileStoreChecks
{
    public static void MemoryDefaultHeadcountProfileStoreFindsWhatItSaved()
    {
        var store = DefaultHeadcountProfileStore.NewMemoryDefaultHeadcountProfileStore();
        var item = DefaultHeadcountProfile.NewDefaultHeadcountProfile("code-419", "label-523");
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
        MemoryDefaultHeadcountProfileStoreFindsWhatItSaved();
    }
}
