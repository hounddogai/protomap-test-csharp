namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAgreementViewChecks
{
    public static void AdaptiveLicenseSeatAgreementViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("code-384", "label-132");
        var view = AdaptiveLicenseSeatAgreementView.ToAdaptiveLicenseSeatAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveLicenseSeatAgreementView.ToAdaptiveLicenseSeatAgreementViews(new List<AdaptiveLicenseSeatAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatAgreementViewShowsTheStatusInCapitals();
    }
}
