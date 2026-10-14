namespace Acme.Workforce.Engagement.Tests.Recognition.Score;

using Acme.Workforce.Engagement.Recognition.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RecognitionPolicyStoreChecks
{
    public static void MemoryRecognitionPolicyStoreFindsWhatItSaved()
    {
        var store = RecognitionPolicyStore.NewMemoryRecognitionPolicyStore();
        var item = RecognitionPolicy.NewRecognitionPolicy("code-70", "title-271");
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
        MemoryRecognitionPolicyStoreFindsWhatItSaved();
    }
}
