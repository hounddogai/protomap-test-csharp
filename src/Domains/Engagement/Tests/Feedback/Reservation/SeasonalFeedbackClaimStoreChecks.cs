namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackClaimStoreChecks
{
    public static void MemorySeasonalFeedbackClaimStoreFindsWhatItSaved()
    {
        var store = SeasonalFeedbackClaimStore.NewMemorySeasonalFeedbackClaimStore();
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("code-619", "title-251");
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
        MemorySeasonalFeedbackClaimStoreFindsWhatItSaved();
    }
}
