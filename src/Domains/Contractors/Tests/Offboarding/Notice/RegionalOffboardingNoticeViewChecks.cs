namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalOffboardingNoticeViewChecks
{
    public static void RegionalOffboardingNoticeViewShowsTheStatusInCapitals()
    {
        var item = RegionalOffboardingNotice.NewRegionalOffboardingNotice("code-392", "label-491");
        var view = RegionalOffboardingNoticeView.ToRegionalOffboardingNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalOffboardingNoticeView.ToRegionalOffboardingNoticeViews(new List<RegionalOffboardingNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalOffboardingNoticeViewShowsTheStatusInCapitals();
    }
}
