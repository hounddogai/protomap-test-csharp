namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class PrimaryEngagementNoticeViewChecks
{
    public static void PrimaryEngagementNoticeViewShowsTheStatusInCapitals()
    {
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("code-642", "label-919");
        var view = PrimaryEngagementNoticeView.ToPrimaryEngagementNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryEngagementNoticeView.ToPrimaryEngagementNoticeViews(new List<PrimaryEngagementNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryEngagementNoticeViewShowsTheStatusInCapitals();
    }
}
