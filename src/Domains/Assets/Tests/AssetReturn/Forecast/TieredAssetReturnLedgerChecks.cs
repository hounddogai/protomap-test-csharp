namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredAssetReturnLedgerChecks
{
    public static void TieredAssetReturnLedgerStartsInDraft()
    {
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("code-206", "title-906");
        Check.Equal(item.Status, TieredAssetReturnLedgerStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAssetReturnLedgerMovesToActive()
    {
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("code-206", "title-906");
        try
        {
            item.MoveTo(TieredAssetReturnLedgerStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAssetReturnLedgerStatus.Active, "status");
    }

    public static void TieredAssetReturnLedgerRejectsMovesFromClosed()
    {
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("code-206", "title-906");
        item.Status = TieredAssetReturnLedgerStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredAssetReturnLedgerStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredAssetReturnLedgerStartsInDraft();
        TieredAssetReturnLedgerMovesToActive();
        TieredAssetReturnLedgerRejectsMovesFromClosed();
    }
}
