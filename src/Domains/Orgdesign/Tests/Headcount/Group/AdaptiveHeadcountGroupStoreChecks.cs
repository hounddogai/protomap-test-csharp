namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveHeadcountGroupStoreChecks
{
    public static void MemoryAdaptiveHeadcountGroupStoreFindsWhatItSaved()
    {
        var store = AdaptiveHeadcountGroupStore.NewMemoryAdaptiveHeadcountGroupStore();
        var item = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup("code-405", "title-713");
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
        MemoryAdaptiveHeadcountGroupStoreFindsWhatItSaved();
    }
}
