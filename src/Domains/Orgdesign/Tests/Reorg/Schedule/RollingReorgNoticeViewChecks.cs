namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingReorgNoticeViewChecks
{
    public static void RollingReorgNoticeViewShowsTheStatusInCapitals()
    {
        var item = RollingReorgNotice.NewRollingReorgNotice("code-717", "label-6");
        var view = RollingReorgNoticeView.ToRollingReorgNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingReorgNoticeView.ToRollingReorgNoticeViews(new List<RollingReorgNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingReorgNoticeViewShowsTheStatusInCapitals();
    }
}
