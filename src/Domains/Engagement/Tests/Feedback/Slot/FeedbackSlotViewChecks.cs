namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class FeedbackSlotViewChecks
{
    public static void FeedbackSlotViewShowsTheStatusInCapitals()
    {
        var item = FeedbackSlot.NewFeedbackSlot("code-226", "name-877");
        var view = FeedbackSlotView.ToFeedbackSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackSlotView.ToFeedbackSlotViews(new List<FeedbackSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackSlotViewShowsTheStatusInCapitals();
    }
}
