namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueScheduleStoreChecks
{
    public static void MemoryTrainingDueScheduleStoreFindsWhatItSaved()
    {
        var store = TrainingDueScheduleStore.NewMemoryTrainingDueScheduleStore();
        var item = TrainingDueSchedule.NewTrainingDueSchedule("code-375", "title-726");
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
        MemoryTrainingDueScheduleStoreFindsWhatItSaved();
    }
}
