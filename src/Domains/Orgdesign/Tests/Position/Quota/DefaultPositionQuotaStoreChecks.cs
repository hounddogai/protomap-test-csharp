namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultPositionQuotaStoreChecks
{
    public static void MemoryDefaultPositionQuotaStoreFindsWhatItSaved()
    {
        var store = DefaultPositionQuotaStore.NewMemoryDefaultPositionQuotaStore();
        var item = DefaultPositionQuota.NewDefaultPositionQuota("code-405", "label-114");
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
        MemoryDefaultPositionQuotaStoreFindsWhatItSaved();
    }
}
