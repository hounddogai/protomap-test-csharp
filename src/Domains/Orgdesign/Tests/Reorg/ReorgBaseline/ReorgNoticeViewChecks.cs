namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

/// <summary>Checks of reorg baseline code, run by the project's runner.</summary>
public static class ReorgNoticeViewChecks
{
    public static void ReorgNoticeViewShowsTheStatusInCapitals()
    {
        var item = ReorgNotice.NewReorgNotice("code-200", "title-493");
        var view = ReorgNoticeView.ToReorgNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgNoticeView.ToReorgNoticeViews(new List<ReorgNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgNoticeViewShowsTheStatusInCapitals();
    }
}
