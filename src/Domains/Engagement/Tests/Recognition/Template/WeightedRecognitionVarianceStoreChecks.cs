namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WeightedRecognitionVarianceStoreChecks
{
    public static void MemoryWeightedRecognitionVarianceStoreFindsWhatItSaved()
    {
        var store = WeightedRecognitionVarianceStore.NewMemoryWeightedRecognitionVarianceStore();
        var item = WeightedRecognitionVariance.NewWeightedRecognitionVariance("code-486", "label-607");
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
        MemoryWeightedRecognitionVarianceStoreFindsWhatItSaved();
    }
}
