namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PrimaryRecognitionPolicyStoreChecks
{
    public static void MemoryPrimaryRecognitionPolicyStoreFindsWhatItSaved()
    {
        var store = PrimaryRecognitionPolicyStore.NewMemoryPrimaryRecognitionPolicyStore();
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("code-132", "label-176");
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
        MemoryPrimaryRecognitionPolicyStoreFindsWhatItSaved();
    }
}
