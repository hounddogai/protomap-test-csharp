namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyQuotaStoreChecks
{
    public static void MemoryAdaptivePolicyQuotaStoreFindsWhatItSaved()
    {
        var store = AdaptivePolicyQuotaStore.NewMemoryAdaptivePolicyQuotaStore();
        var item = AdaptivePolicyQuota.NewAdaptivePolicyQuota("code-330", "title-595");
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
        MemoryAdaptivePolicyQuotaStoreFindsWhatItSaved();
    }
}
