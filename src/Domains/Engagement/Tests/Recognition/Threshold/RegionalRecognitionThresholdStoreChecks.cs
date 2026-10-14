namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalRecognitionThresholdStoreChecks
{
    public static void MemoryRegionalRecognitionThresholdStoreFindsWhatItSaved()
    {
        var store = RegionalRecognitionThresholdStore.NewMemoryRegionalRecognitionThresholdStore();
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("code-150", "label-902");
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
        MemoryRegionalRecognitionThresholdStoreFindsWhatItSaved();
    }
}
