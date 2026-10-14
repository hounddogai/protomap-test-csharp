namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GlobalAssetReturnRecordViewChecks
{
    public static void GlobalAssetReturnRecordViewShowsTheStatusInCapitals()
    {
        var item = GlobalAssetReturnRecord.NewGlobalAssetReturnRecord("code-611", "name-658");
        var view = GlobalAssetReturnRecordView.ToGlobalAssetReturnRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAssetReturnRecordView.ToGlobalAssetReturnRecordViews(new List<GlobalAssetReturnRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAssetReturnRecordViewShowsTheStatusInCapitals();
    }
}
