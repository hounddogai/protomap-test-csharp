namespace Acme.Workforce.Engagement.Tests.Feedback.Claim;

using Acme.Workforce.Engagement.Feedback.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryFeedbackClaimStoreChecks
{
    public static void MemoryPrimaryFeedbackClaimStoreFindsWhatItSaved()
    {
        var store = PrimaryFeedbackClaimStore.NewMemoryPrimaryFeedbackClaimStore();
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("code-806", "title-204");
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
        MemoryPrimaryFeedbackClaimStoreFindsWhatItSaved();
    }
}
