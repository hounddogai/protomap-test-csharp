namespace Acme.Workforce.Compensation.Tests.MeritCycle.Request;

using Acme.Workforce.Compensation.MeritCycle.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictMeritCyclePlanStoreChecks
{
    public static void MemoryStrictMeritCyclePlanStoreFindsWhatItSaved()
    {
        var store = StrictMeritCyclePlanStore.NewMemoryStrictMeritCyclePlanStore();
        var item = StrictMeritCyclePlan.NewStrictMeritCyclePlan("code-180", "name-81");
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
        MemoryStrictMeritCyclePlanStoreFindsWhatItSaved();
    }
}
