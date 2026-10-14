namespace Acme.Workforce.Orgdesign.Tests.Reorg.Rule;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictReorgNoticeViewChecks
{
    public static void StrictReorgNoticeViewShowsTheStatusInCapitals()
    {
        var item = StrictReorgNotice.NewStrictReorgNotice("code-800", "label-717");
        var view = StrictReorgNoticeView.ToStrictReorgNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictReorgNoticeView.ToStrictReorgNoticeViews(new List<StrictReorgNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictReorgNoticeViewShowsTheStatusInCapitals();
    }
}
