namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueMetricStoreChecks
{
    public static void MemoryTrainingDueMetricStoreFindsWhatItSaved()
    {
        var store = TrainingDueMetricStore.NewMemoryTrainingDueMetricStore();
        var item = TrainingDueMetric.NewTrainingDueMetric("code-376", "title-902");
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
        MemoryTrainingDueMetricStoreFindsWhatItSaved();
    }
}
