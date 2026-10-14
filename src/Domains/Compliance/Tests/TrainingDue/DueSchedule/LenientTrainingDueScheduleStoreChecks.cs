namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class LenientTrainingDueScheduleStoreChecks
{
    public static void MemoryLenientTrainingDueScheduleStoreFindsWhatItSaved()
    {
        var store = LenientTrainingDueScheduleStore.NewMemoryLenientTrainingDueScheduleStore();
        var item = LenientTrainingDueSchedule.NewLenientTrainingDueSchedule("code-648", "label-466");
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
        MemoryLenientTrainingDueScheduleStoreFindsWhatItSaved();
    }
}
