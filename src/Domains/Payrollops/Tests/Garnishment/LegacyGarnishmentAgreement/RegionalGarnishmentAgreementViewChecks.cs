namespace Acme.Workforce.Payrollops.Tests.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Checks of legacy garnishment agreement code, run by the project's runner.</summary>
public static class RegionalGarnishmentAgreementViewChecks
{
    public static void RegionalGarnishmentAgreementViewShowsTheStatusInCapitals()
    {
        var item = RegionalGarnishmentAgreement.NewRegionalGarnishmentAgreement("code-520", "label-468");
        var view = RegionalGarnishmentAgreementView.ToRegionalGarnishmentAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalGarnishmentAgreementView.ToRegionalGarnishmentAgreementViews(new List<RegionalGarnishmentAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalGarnishmentAgreementViewShowsTheStatusInCapitals();
    }
}
