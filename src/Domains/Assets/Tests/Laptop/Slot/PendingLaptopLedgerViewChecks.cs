namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PendingLaptopLedgerViewChecks
{
    public static void PendingLaptopLedgerViewShowsTheStatusInCapitals()
    {
        var item = PendingLaptopLedger.NewPendingLaptopLedger("code-853", "name-562");
        var view = PendingLaptopLedgerView.ToPendingLaptopLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingLaptopLedgerView.ToPendingLaptopLedgerViews(new List<PendingLaptopLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingLaptopLedgerViewShowsTheStatusInCapitals();
    }
}
