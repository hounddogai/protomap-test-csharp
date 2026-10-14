namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantNoticeViewChecks
{
    public static void EquityGrantNoticeViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantNotice.NewEquityGrantNotice("code-617", "title-243");
        var view = EquityGrantNoticeView.ToEquityGrantNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantNoticeView.ToEquityGrantNoticeViews(new List<EquityGrantNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantNoticeViewShowsTheStatusInCapitals();
    }
}
