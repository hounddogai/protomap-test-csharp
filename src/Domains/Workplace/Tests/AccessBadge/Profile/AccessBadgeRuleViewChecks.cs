namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeRuleViewChecks
{
    public static void AccessBadgeRuleViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeRule.NewAccessBadgeRule("code-585", "label-661");
        var view = AccessBadgeRuleView.ToAccessBadgeRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeRuleView.ToAccessBadgeRuleViews(new List<AccessBadgeRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeRuleViewShowsTheStatusInCapitals();
    }
}
