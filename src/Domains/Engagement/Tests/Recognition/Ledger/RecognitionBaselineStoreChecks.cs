namespace Acme.Workforce.Engagement.Tests.Recognition.Ledger;

using Acme.Workforce.Engagement.Recognition.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RecognitionBaselineStoreChecks
{
    public static void MemoryRecognitionBaselineStoreFindsWhatItSaved()
    {
        var store = RecognitionBaselineStore.NewMemoryRecognitionBaselineStore();
        var item = RecognitionBaseline.NewRecognitionBaseline("code-970", "name-437");
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
        MemoryRecognitionBaselineStoreFindsWhatItSaved();
    }
}
