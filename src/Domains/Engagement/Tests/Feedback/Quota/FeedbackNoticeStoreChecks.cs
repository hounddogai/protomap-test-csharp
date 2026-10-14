namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackNoticeStoreChecks
{
    public static void MemoryFeedbackNoticeStoreFindsWhatItSaved()
    {
        var store = FeedbackNoticeStore.NewMemoryFeedbackNoticeStore();
        var item = FeedbackNotice.NewFeedbackNotice("code-465", "label-478");
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
        MemoryFeedbackNoticeStoreFindsWhatItSaved();
    }
}
