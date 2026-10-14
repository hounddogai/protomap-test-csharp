namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountOverrideStoreChecks
{
    public static void MemoryHeadcountOverrideStoreFindsWhatItSaved()
    {
        var store = HeadcountOverrideStore.NewMemoryHeadcountOverrideStore();
        var item = HeadcountOverride.NewHeadcountOverride("code-568", "label-117");
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
        MemoryHeadcountOverrideStoreFindsWhatItSaved();
    }
}
