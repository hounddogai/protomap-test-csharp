namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PendingLaptopLedgerChecks
{
    public static void PendingLaptopLedgerStartsInRequested()
    {
        var item = PendingLaptopLedger.NewPendingLaptopLedger("code-853", "name-562");
        Check.Equal(item.Status, PendingLaptopLedgerStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingLaptopLedgerMovesToRevoked()
    {
        var item = PendingLaptopLedger.NewPendingLaptopLedger("code-853", "name-562");
        try
        {
            item.MoveTo(PendingLaptopLedgerStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingLaptopLedgerStatus.Revoked, "status");
    }

    public static void PendingLaptopLedgerRejectsMovesFromExpired()
    {
        var item = PendingLaptopLedger.NewPendingLaptopLedger("code-853", "name-562");
        item.Status = PendingLaptopLedgerStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingLaptopLedgerStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingLaptopLedgerStartsInRequested();
        PendingLaptopLedgerMovesToRevoked();
        PendingLaptopLedgerRejectsMovesFromExpired();
    }
}
