namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class StrictPolicyEntryViewChecks
{
    public static void StrictPolicyEntryViewShowsTheStatusInCapitals()
    {
        var item = StrictPolicyEntry.NewStrictPolicyEntry("code-352", "name-282");
        var view = StrictPolicyEntryView.ToStrictPolicyEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPolicyEntryView.ToStrictPolicyEntryViews(new List<StrictPolicyEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPolicyEntryViewShowsTheStatusInCapitals();
    }
}
