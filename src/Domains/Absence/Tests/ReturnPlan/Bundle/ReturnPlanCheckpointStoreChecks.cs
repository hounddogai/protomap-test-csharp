namespace Acme.Workforce.Absence.Tests.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReturnPlanCheckpointStoreChecks
{
    public static void MemoryReturnPlanCheckpointStoreFindsWhatItSaved()
    {
        var store = ReturnPlanCheckpointStore.NewMemoryReturnPlanCheckpointStore();
        var item = ReturnPlanCheckpoint.NewReturnPlanCheckpoint("code-55", "title-866");
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
        MemoryReturnPlanCheckpointStoreFindsWhatItSaved();
    }
}
