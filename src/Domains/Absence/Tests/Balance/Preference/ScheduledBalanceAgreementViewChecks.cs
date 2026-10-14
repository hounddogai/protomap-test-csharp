namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledBalanceAgreementViewChecks
{
    public static void ScheduledBalanceAgreementViewShowsTheStatusInCapitals()
    {
        var item = ScheduledBalanceAgreement.NewScheduledBalanceAgreement("code-503", "title-690");
        var view = ScheduledBalanceAgreementView.ToScheduledBalanceAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledBalanceAgreementView.ToScheduledBalanceAgreementViews(new List<ScheduledBalanceAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledBalanceAgreementViewShowsTheStatusInCapitals();
    }
}
