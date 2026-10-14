namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class NominationAgreementViewChecks
{
    public static void NominationAgreementViewShowsTheStatusInCapitals()
    {
        var item = NominationAgreement.NewNominationAgreement("code-412", "title-100");
        var view = NominationAgreementView.ToNominationAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationAgreementView.ToNominationAgreementViews(new List<NominationAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationAgreementViewShowsTheStatusInCapitals();
    }
}
