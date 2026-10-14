namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopLedgerChecks
{
    public static void LaptopLedgerStartsInIdle()
    {
        var item = LaptopLedger.NewLaptopLedger("code-693", "label-997");
        Check.Equal(item.Status, LaptopLedgerStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopLedgerMovesToStopped()
    {
        var item = LaptopLedger.NewLaptopLedger("code-693", "label-997");
        try
        {
            item.MoveTo(LaptopLedgerStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopLedgerStatus.Stopped, "status");
    }

    public static void LaptopLedgerRejectsMovesFromStopped()
    {
        var item = LaptopLedger.NewLaptopLedger("code-693", "label-997");
        item.Status = LaptopLedgerStatus.Stopped;
        Check.Fails(() => item.MoveTo(LaptopLedgerStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LaptopLedgerStartsInIdle();
        LaptopLedgerMovesToStopped();
        LaptopLedgerRejectsMovesFromStopped();
    }
}
