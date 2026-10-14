namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountVarianceStoreChecks
{
    public static void MemoryHeadcountVarianceStoreFindsWhatItSaved()
    {
        var store = HeadcountVarianceStore.NewMemoryHeadcountVarianceStore();
        var item = HeadcountVariance.NewHeadcountVariance("code-765", "label-685");
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
        MemoryHeadcountVarianceStoreFindsWhatItSaved();
    }
}
