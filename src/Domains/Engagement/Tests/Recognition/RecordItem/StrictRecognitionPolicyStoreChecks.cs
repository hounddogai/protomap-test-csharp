namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictRecognitionPolicyStoreChecks
{
    public static void MemoryStrictRecognitionPolicyStoreFindsWhatItSaved()
    {
        var store = StrictRecognitionPolicyStore.NewMemoryStrictRecognitionPolicyStore();
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("code-36", "name-84");
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
        MemoryStrictRecognitionPolicyStoreFindsWhatItSaved();
    }
}
