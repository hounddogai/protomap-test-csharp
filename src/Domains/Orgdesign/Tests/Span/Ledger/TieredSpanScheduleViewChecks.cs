namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TieredSpanScheduleViewChecks
{
    public static void TieredSpanScheduleViewShowsTheStatusInCapitals()
    {
        var item = TieredSpanSchedule.NewTieredSpanSchedule("code-728", "name-94");
        var view = TieredSpanScheduleView.ToTieredSpanScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredSpanScheduleView.ToTieredSpanScheduleViews(new List<TieredSpanSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredSpanScheduleViewShowsTheStatusInCapitals();
    }
}
