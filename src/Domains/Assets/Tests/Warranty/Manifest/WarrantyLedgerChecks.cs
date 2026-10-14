namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyLedgerChecks
{
    public static void WarrantyLedgerStartsInOpen()
    {
        var item = WarrantyLedger.NewWarrantyLedger("code-442", "title-998");
        Check.Equal(item.Status, WarrantyLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyLedgerMovesToReopened()
    {
        var item = WarrantyLedger.NewWarrantyLedger("code-442", "title-998");
        try
        {
            item.MoveTo(WarrantyLedgerStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyLedgerStatus.Reopened, "status");
    }

    public static void WarrantyLedgerRejectsMovesFromArchived()
    {
        var item = WarrantyLedger.NewWarrantyLedger("code-442", "title-998");
        item.Status = WarrantyLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(WarrantyLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        WarrantyLedgerStartsInOpen();
        WarrantyLedgerMovesToReopened();
        WarrantyLedgerRejectsMovesFromArchived();
    }
}
