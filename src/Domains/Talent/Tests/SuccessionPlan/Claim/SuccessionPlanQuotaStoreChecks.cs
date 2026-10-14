namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SuccessionPlanQuotaStoreChecks
{
    public static void MemorySuccessionPlanQuotaStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanQuotaStore.NewMemorySuccessionPlanQuotaStore();
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("code-924", "title-683");
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
        MemorySuccessionPlanQuotaStoreFindsWhatItSaved();
    }
}
