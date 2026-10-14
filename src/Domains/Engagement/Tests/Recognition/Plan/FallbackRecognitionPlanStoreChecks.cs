namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class FallbackRecognitionPlanStoreChecks
{
    public static void MemoryFallbackRecognitionPlanStoreFindsWhatItSaved()
    {
        var store = FallbackRecognitionPlanStore.NewMemoryFallbackRecognitionPlanStore();
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("code-325", "label-769");
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
        MemoryFallbackRecognitionPlanStoreFindsWhatItSaved();
    }
}
