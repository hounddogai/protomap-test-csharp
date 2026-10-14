namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RecognitionMetricStoreChecks
{
    public static void MemoryRecognitionMetricStoreFindsWhatItSaved()
    {
        var store = RecognitionMetricStore.NewMemoryRecognitionMetricStore();
        var item = RecognitionMetric.NewRecognitionMetric("code-746", "name-600");
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
        MemoryRecognitionMetricStoreFindsWhatItSaved();
    }
}
