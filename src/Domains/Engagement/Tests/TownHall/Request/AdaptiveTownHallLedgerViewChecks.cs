namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptiveTownHallLedgerViewChecks
{
    public static void AdaptiveTownHallLedgerViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("code-324", "name-96");
        var view = AdaptiveTownHallLedgerView.ToAdaptiveTownHallLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTownHallLedgerView.ToAdaptiveTownHallLedgerViews(new List<AdaptiveTownHallLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTownHallLedgerViewShowsTheStatusInCapitals();
    }
}
