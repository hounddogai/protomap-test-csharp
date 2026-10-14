namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleNoticeViewChecks
{
    public static void MeritCycleNoticeViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleNotice.NewMeritCycleNotice("code-186", "label-683");
        var view = MeritCycleNoticeView.ToMeritCycleNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleNoticeView.ToMeritCycleNoticeViews(new List<MeritCycleNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleNoticeViewShowsTheStatusInCapitals();
    }
}
