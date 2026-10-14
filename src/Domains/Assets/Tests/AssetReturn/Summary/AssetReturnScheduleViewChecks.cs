namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnScheduleViewChecks
{
    public static void AssetReturnScheduleViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnSchedule.NewAssetReturnSchedule("code-482", "title-836");
        var view = AssetReturnScheduleView.ToAssetReturnScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnScheduleView.ToAssetReturnScheduleViews(new List<AssetReturnSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnScheduleViewShowsTheStatusInCapitals();
    }
}
