namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyLedgerViewChecks
{
    public static void WarrantyLedgerViewShowsTheStatusInCapitals()
    {
        var item = WarrantyLedger.NewWarrantyLedger("code-442", "title-998");
        var view = WarrantyLedgerView.ToWarrantyLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyLedgerView.ToWarrantyLedgerViews(new List<WarrantyLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyLedgerViewShowsTheStatusInCapitals();
    }
}
