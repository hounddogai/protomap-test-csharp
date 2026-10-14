namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyRecognitionBudgetStoreChecks
{
    public static void MemoryLegacyRecognitionBudgetStoreFindsWhatItSaved()
    {
        var store = LegacyRecognitionBudgetStore.NewMemoryLegacyRecognitionBudgetStore();
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("code-878", "name-75");
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
        MemoryLegacyRecognitionBudgetStoreFindsWhatItSaved();
    }
}
