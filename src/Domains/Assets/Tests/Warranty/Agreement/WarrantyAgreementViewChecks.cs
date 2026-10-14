namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyAgreementViewChecks
{
    public static void WarrantyAgreementViewShowsTheStatusInCapitals()
    {
        var item = WarrantyAgreement.NewWarrantyAgreement("code-917", "title-106");
        var view = WarrantyAgreementView.ToWarrantyAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyAgreementView.ToWarrantyAgreementViews(new List<WarrantyAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyAgreementViewShowsTheStatusInCapitals();
    }
}
