namespace Acme.Workforce.Engagement.Tests.Recognition.Claim;

using Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RecognitionClaimStoreChecks
{
    public static void MemoryRecognitionClaimStoreFindsWhatItSaved()
    {
        var store = RecognitionClaimStore.NewMemoryRecognitionClaimStore();
        var item = RecognitionClaim.NewRecognitionClaim("code-755", "name-368");
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
        MemoryRecognitionClaimStoreFindsWhatItSaved();
    }
}
