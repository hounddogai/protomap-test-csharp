namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TownHallSummaryViewChecks
{
    public static void TownHallSummaryViewShowsTheStatusInCapitals()
    {
        var item = TownHallSummary.NewTownHallSummary("code-245", "label-157");
        var view = TownHallSummaryView.ToTownHallSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallSummaryView.ToTownHallSummaryViews(new List<TownHallSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallSummaryViewShowsTheStatusInCapitals();
    }
}
