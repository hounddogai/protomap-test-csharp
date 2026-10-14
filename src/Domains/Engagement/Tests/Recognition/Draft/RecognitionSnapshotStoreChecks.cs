namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RecognitionSnapshotStoreChecks
{
    public static void MemoryRecognitionSnapshotStoreFindsWhatItSaved()
    {
        var store = RecognitionSnapshotStore.NewMemoryRecognitionSnapshotStore();
        var item = RecognitionSnapshot.NewRecognitionSnapshot("code-380", "label-895");
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
        MemoryRecognitionSnapshotStoreFindsWhatItSaved();
    }
}
