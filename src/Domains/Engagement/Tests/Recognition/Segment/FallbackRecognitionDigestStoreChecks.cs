namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackRecognitionDigestStoreChecks
{
    public static void MemoryFallbackRecognitionDigestStoreFindsWhatItSaved()
    {
        var store = FallbackRecognitionDigestStore.NewMemoryFallbackRecognitionDigestStore();
        var item = FallbackRecognitionDigest.NewFallbackRecognitionDigest("code-721", "label-133");
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
        MemoryFallbackRecognitionDigestStoreFindsWhatItSaved();
    }
}
