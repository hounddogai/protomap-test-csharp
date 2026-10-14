namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualPreferenceViewChecks
{
    public static void AccrualPreferenceViewShowsTheStatusInCapitals()
    {
        var item = AccrualPreference.NewAccrualPreference("code-785", "title-197");
        var view = AccrualPreferenceView.ToAccrualPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualPreferenceView.ToAccrualPreferenceViews(new List<AccrualPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualPreferenceViewShowsTheStatusInCapitals();
    }
}
