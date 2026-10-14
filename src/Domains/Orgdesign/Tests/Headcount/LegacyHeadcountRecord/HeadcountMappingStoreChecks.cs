namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class HeadcountMappingStoreChecks
{
    public static void MemoryHeadcountMappingStoreFindsWhatItSaved()
    {
        var store = HeadcountMappingStore.NewMemoryHeadcountMappingStore();
        var item = HeadcountMapping.NewHeadcountMapping("code-972", "label-124");
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
        MemoryHeadcountMappingStoreFindsWhatItSaved();
    }
}
