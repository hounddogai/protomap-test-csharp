namespace Acme.Workforce.Absence.Tests.ReturnPlan.Schedule;

using Acme.Workforce.Absence.ReturnPlan.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReturnPlanThresholdStoreChecks
{
    public static void MemoryReturnPlanThresholdStoreFindsWhatItSaved()
    {
        var store = ReturnPlanThresholdStore.NewMemoryReturnPlanThresholdStore();
        var item = ReturnPlanThreshold.NewReturnPlanThreshold("code-988", "title-497");
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
        MemoryReturnPlanThresholdStoreFindsWhatItSaved();
    }
}
