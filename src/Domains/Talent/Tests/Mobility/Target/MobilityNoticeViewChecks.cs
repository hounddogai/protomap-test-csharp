namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityNoticeViewChecks
{
    public static void MobilityNoticeViewShowsTheStatusInCapitals()
    {
        var item = MobilityNotice.NewMobilityNotice("code-647", "name-250");
        var view = MobilityNoticeView.ToMobilityNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityNoticeView.ToMobilityNoticeViews(new List<MobilityNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityNoticeViewShowsTheStatusInCapitals();
    }
}
