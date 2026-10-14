namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultRecognitionPolicyStoreChecks
{
    public static void MemoryDefaultRecognitionPolicyStoreFindsWhatItSaved()
    {
        var store = DefaultRecognitionPolicyStore.NewMemoryDefaultRecognitionPolicyStore();
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("code-947", "name-928");
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
        MemoryDefaultRecognitionPolicyStoreFindsWhatItSaved();
    }
}
