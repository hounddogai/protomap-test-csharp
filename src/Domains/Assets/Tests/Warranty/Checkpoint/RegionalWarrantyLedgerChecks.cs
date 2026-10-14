namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalWarrantyLedgerChecks
{
    public static void RegionalWarrantyLedgerStartsInNew()
    {
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("code-688", "title-360");
        Check.Equal(item.Status, RegionalWarrantyLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalWarrantyLedgerMovesToQueued()
    {
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("code-688", "title-360");
        try
        {
            item.MoveTo(RegionalWarrantyLedgerStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalWarrantyLedgerStatus.Queued, "status");
    }

    public static void RegionalWarrantyLedgerRejectsMovesFromFailed()
    {
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("code-688", "title-360");
        item.Status = RegionalWarrantyLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalWarrantyLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalWarrantyLedgerStartsInNew();
        RegionalWarrantyLedgerMovesToQueued();
        RegionalWarrantyLedgerRejectsMovesFromFailed();
    }
}
