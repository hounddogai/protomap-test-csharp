namespace Acme.Workforce.Engagement.Tests.Recognition.Digest;

using Acme.Workforce.Engagement.Recognition.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RecognitionDigestStoreChecks
{
    public static void MemoryRecognitionDigestStoreFindsWhatItSaved()
    {
        var store = RecognitionDigestStore.NewMemoryRecognitionDigestStore();
        var item = RecognitionDigest.NewRecognitionDigest("code-235", "title-504");
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
        MemoryRecognitionDigestStoreFindsWhatItSaved();
    }
}
