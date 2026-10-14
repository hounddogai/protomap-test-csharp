namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalWarrantyLedgerViewChecks
{
    public static void RegionalWarrantyLedgerViewShowsTheStatusInCapitals()
    {
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("code-688", "title-360");
        var view = RegionalWarrantyLedgerView.ToRegionalWarrantyLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalWarrantyLedgerView.ToRegionalWarrantyLedgerViews(new List<RegionalWarrantyLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalWarrantyLedgerViewShowsTheStatusInCapitals();
    }
}
