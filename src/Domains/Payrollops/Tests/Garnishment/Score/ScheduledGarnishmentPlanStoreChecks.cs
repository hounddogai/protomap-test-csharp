namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledGarnishmentPlanStoreChecks
{
    public static void MemoryScheduledGarnishmentPlanStoreFindsWhatItSaved()
    {
        var store = ScheduledGarnishmentPlanStore.NewMemoryScheduledGarnishmentPlanStore();
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("code-702", "title-667");
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
        MemoryScheduledGarnishmentPlanStoreFindsWhatItSaved();
    }
}
