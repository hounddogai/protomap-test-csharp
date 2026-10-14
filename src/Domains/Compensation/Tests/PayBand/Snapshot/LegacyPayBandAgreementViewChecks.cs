namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyPayBandAgreementViewChecks
{
    public static void LegacyPayBandAgreementViewShowsTheStatusInCapitals()
    {
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("code-235", "label-214");
        var view = LegacyPayBandAgreementView.ToLegacyPayBandAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPayBandAgreementView.ToLegacyPayBandAgreementViews(new List<LegacyPayBandAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPayBandAgreementViewShowsTheStatusInCapitals();
    }
}
