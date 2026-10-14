namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallBatchViewChecks
{
    public static void TownHallBatchViewShowsTheStatusInCapitals()
    {
        var item = TownHallBatch.NewTownHallBatch("code-474", "title-779");
        var view = TownHallBatchView.ToTownHallBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallBatchView.ToTownHallBatchViews(new List<TownHallBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallBatchViewShowsTheStatusInCapitals();
    }
}
