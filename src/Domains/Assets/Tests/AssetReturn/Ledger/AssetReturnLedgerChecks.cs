namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AssetReturnLedgerChecks
{
    public static void AssetReturnLedgerStartsInCreated()
    {
        var item = AssetReturnLedger.NewAssetReturnLedger("code-31", "title-673");
        Check.Equal(item.Status, AssetReturnLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnLedgerMovesToPublished()
    {
        var item = AssetReturnLedger.NewAssetReturnLedger("code-31", "title-673");
        try
        {
            item.MoveTo(AssetReturnLedgerStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnLedgerStatus.Published, "status");
    }

    public static void AssetReturnLedgerRejectsMovesFromRetired()
    {
        var item = AssetReturnLedger.NewAssetReturnLedger("code-31", "title-673");
        item.Status = AssetReturnLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(AssetReturnLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssetReturnLedgerStartsInCreated();
        AssetReturnLedgerMovesToPublished();
        AssetReturnLedgerRejectsMovesFromRetired();
    }
}
