namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgePreferenceViewChecks
{
    public static void AccessBadgePreferenceViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgePreference.NewAccessBadgePreference("code-246", "title-473");
        var view = AccessBadgePreferenceView.ToAccessBadgePreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgePreferenceView.ToAccessBadgePreferenceViews(new List<AccessBadgePreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgePreferenceViewShowsTheStatusInCapitals();
    }
}
