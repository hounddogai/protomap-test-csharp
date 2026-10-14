namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ManualEngagementRuleViewChecks
{
    public static void ManualEngagementRuleViewShowsTheStatusInCapitals()
    {
        var item = ManualEngagementRule.NewManualEngagementRule("code-974", "label-118");
        var view = ManualEngagementRuleView.ToManualEngagementRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualEngagementRuleView.ToManualEngagementRuleViews(new List<ManualEngagementRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualEngagementRuleViewShowsTheStatusInCapitals();
    }
}
