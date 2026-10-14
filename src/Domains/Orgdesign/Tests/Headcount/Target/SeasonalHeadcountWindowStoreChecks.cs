namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalHeadcountWindowStoreChecks
{
    public static void MemorySeasonalHeadcountWindowStoreFindsWhatItSaved()
    {
        var store = SeasonalHeadcountWindowStore.NewMemorySeasonalHeadcountWindowStore();
        var item = SeasonalHeadcountWindow.NewSeasonalHeadcountWindow("code-302", "label-369");
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
        MemorySeasonalHeadcountWindowStoreFindsWhatItSaved();
    }
}
