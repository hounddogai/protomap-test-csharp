namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictReturnPlanBundleStoreChecks
{
    public static void MemoryStrictReturnPlanBundleStoreFindsWhatItSaved()
    {
        var store = StrictReturnPlanBundleStore.NewMemoryStrictReturnPlanBundleStore();
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("code-401", "name-834");
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
        MemoryStrictReturnPlanBundleStoreFindsWhatItSaved();
    }
}
