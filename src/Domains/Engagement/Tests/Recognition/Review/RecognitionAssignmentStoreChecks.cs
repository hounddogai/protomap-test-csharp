namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RecognitionAssignmentStoreChecks
{
    public static void MemoryRecognitionAssignmentStoreFindsWhatItSaved()
    {
        var store = RecognitionAssignmentStore.NewMemoryRecognitionAssignmentStore();
        var item = RecognitionAssignment.NewRecognitionAssignment("code-104", "title-499");
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
        MemoryRecognitionAssignmentStoreFindsWhatItSaved();
    }
}
