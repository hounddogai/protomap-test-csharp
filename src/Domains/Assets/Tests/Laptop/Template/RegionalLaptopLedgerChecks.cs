namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RegionalLaptopLedgerChecks
{
    public static void RegionalLaptopLedgerStartsInNew()
    {
        var item = RegionalLaptopLedger.NewRegionalLaptopLedger("code-593", "title-669");
        Check.Equal(item.Status, RegionalLaptopLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalLaptopLedgerMovesToFailed()
    {
        var item = RegionalLaptopLedger.NewRegionalLaptopLedger("code-593", "title-669");
        try
        {
            item.MoveTo(RegionalLaptopLedgerStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalLaptopLedgerStatus.Failed, "status");
    }

    public static void RegionalLaptopLedgerRejectsMovesFromFailed()
    {
        var item = RegionalLaptopLedger.NewRegionalLaptopLedger("code-593", "title-669");
        item.Status = RegionalLaptopLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalLaptopLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalLaptopLedgerStartsInNew();
        RegionalLaptopLedgerMovesToFailed();
        RegionalLaptopLedgerRejectsMovesFromFailed();
    }
}
