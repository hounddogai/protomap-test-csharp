namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class GlobalCostModelQuotaStoreChecks
{
    public static void MemoryGlobalCostModelQuotaStoreFindsWhatItSaved()
    {
        var store = GlobalCostModelQuotaStore.NewMemoryGlobalCostModelQuotaStore();
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("code-726", "name-452");
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
        MemoryGlobalCostModelQuotaStoreFindsWhatItSaved();
    }
}
