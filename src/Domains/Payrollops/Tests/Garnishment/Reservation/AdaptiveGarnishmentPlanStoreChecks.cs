namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentPlanStoreChecks
{
    public static void MemoryAdaptiveGarnishmentPlanStoreFindsWhatItSaved()
    {
        var store = AdaptiveGarnishmentPlanStore.NewMemoryAdaptiveGarnishmentPlanStore();
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("code-184", "title-607");
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
        MemoryAdaptiveGarnishmentPlanStoreFindsWhatItSaved();
    }
}
