namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackGarnishmentPlanStoreChecks
{
    public static void MemoryFallbackGarnishmentPlanStoreFindsWhatItSaved()
    {
        var store = FallbackGarnishmentPlanStore.NewMemoryFallbackGarnishmentPlanStore();
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("code-790", "title-535");
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
        MemoryFallbackGarnishmentPlanStoreFindsWhatItSaved();
    }
}
