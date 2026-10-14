namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class GlobalTrainingDueForecastStoreChecks
{
    public static void MemoryGlobalTrainingDueForecastStoreFindsWhatItSaved()
    {
        var store = GlobalTrainingDueForecastStore.NewMemoryGlobalTrainingDueForecastStore();
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("code-715", "title-847");
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
        MemoryGlobalTrainingDueForecastStoreFindsWhatItSaved();
    }
}
