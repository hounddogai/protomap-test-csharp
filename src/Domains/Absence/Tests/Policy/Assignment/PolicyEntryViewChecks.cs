namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyEntryViewChecks
{
    public static void PolicyEntryViewShowsTheStatusInCapitals()
    {
        var item = PolicyEntry.NewPolicyEntry("code-103", "name-270");
        var view = PolicyEntryView.ToPolicyEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyEntryView.ToPolicyEntryViews(new List<PolicyEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyEntryViewShowsTheStatusInCapitals();
    }
}
