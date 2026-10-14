namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class TieredPayRunMilestoneStoreChecks
{
    public static void MemoryTieredPayRunMilestoneStoreFindsWhatItSaved()
    {
        var store = TieredPayRunMilestoneStore.NewMemoryTieredPayRunMilestoneStore();
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("code-648", "label-331");
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
        MemoryTieredPayRunMilestoneStoreFindsWhatItSaved();
    }
}
