namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BatchedTownHallNoticeViewChecks
{
    public static void BatchedTownHallNoticeViewShowsTheStatusInCapitals()
    {
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("code-911", "name-594");
        var view = BatchedTownHallNoticeView.ToBatchedTownHallNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTownHallNoticeView.ToBatchedTownHallNoticeViews(new List<BatchedTownHallNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTownHallNoticeViewShowsTheStatusInCapitals();
    }
}
