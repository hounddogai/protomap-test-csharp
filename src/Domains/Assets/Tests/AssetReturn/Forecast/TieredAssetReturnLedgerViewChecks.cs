namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredAssetReturnLedgerViewChecks
{
    public static void TieredAssetReturnLedgerViewShowsTheStatusInCapitals()
    {
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("code-206", "title-906");
        var view = TieredAssetReturnLedgerView.ToTieredAssetReturnLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredAssetReturnLedgerView.ToTieredAssetReturnLedgerViews(new List<TieredAssetReturnLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredAssetReturnLedgerViewShowsTheStatusInCapitals();
    }
}
