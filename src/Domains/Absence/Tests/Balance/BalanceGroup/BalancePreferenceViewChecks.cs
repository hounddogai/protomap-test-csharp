namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalancePreferenceViewChecks
{
    public static void BalancePreferenceViewShowsTheStatusInCapitals()
    {
        var item = BalancePreference.NewBalancePreference("code-393", "title-884");
        var view = BalancePreferenceView.ToBalancePreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalancePreferenceView.ToBalancePreferenceViews(new List<BalancePreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalancePreferenceViewShowsTheStatusInCapitals();
    }
}
