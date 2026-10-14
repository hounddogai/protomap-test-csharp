namespace Acme.Workforce.Orgdesign.Tests.Headcount.Notice;

using Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class HeadcountNoticeViewChecks
{
    public static void HeadcountNoticeViewShowsTheStatusInCapitals()
    {
        var item = HeadcountNotice.NewHeadcountNotice("code-706", "label-358");
        var view = HeadcountNoticeView.ToHeadcountNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountNoticeView.ToHeadcountNoticeViews(new List<HeadcountNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountNoticeViewShowsTheStatusInCapitals();
    }
}
