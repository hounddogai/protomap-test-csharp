namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class AssetReturnEntryViewChecks
{
    public static void AssetReturnEntryViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnEntry.NewAssetReturnEntry("code-284", "title-512");
        var view = AssetReturnEntryView.ToAssetReturnEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnEntryView.ToAssetReturnEntryViews(new List<AssetReturnEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnEntryViewShowsTheStatusInCapitals();
    }
}
