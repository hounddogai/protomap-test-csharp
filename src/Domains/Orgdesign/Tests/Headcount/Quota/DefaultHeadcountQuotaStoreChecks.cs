namespace Acme.Workforce.Orgdesign.Tests.Headcount.Quota;

using Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultHeadcountQuotaStoreChecks
{
    public static void MemoryDefaultHeadcountQuotaStoreFindsWhatItSaved()
    {
        var store = DefaultHeadcountQuotaStore.NewMemoryDefaultHeadcountQuotaStore();
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("code-241", "name-616");
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
        MemoryDefaultHeadcountQuotaStoreFindsWhatItSaved();
    }
}
