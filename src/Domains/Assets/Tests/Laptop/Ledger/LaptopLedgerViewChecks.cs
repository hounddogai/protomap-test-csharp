namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopLedgerViewChecks
{
    public static void LaptopLedgerViewShowsTheStatusInCapitals()
    {
        var item = LaptopLedger.NewLaptopLedger("code-693", "label-997");
        var view = LaptopLedgerView.ToLaptopLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopLedgerView.ToLaptopLedgerViews(new List<LaptopLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopLedgerViewShowsTheStatusInCapitals();
    }
}
