namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a asset return group.</summary>
public sealed record AssetReturnGroupView(string CodeText, string Title, string Version, string BatchSizeText, string GraceDaysText, string StatusLabel)
{
    public static AssetReturnGroupView ToAssetReturnGroupView(AssetReturnGroup item)
    {
        return new AssetReturnGroupView(
            item.Code,
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnGroupView> ToAssetReturnGroupViews(List<AssetReturnGroup> items)
    {
        var views = new List<AssetReturnGroupView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnGroupView(item));
        }
        return views;
    }

    public static double SumAssetReturnGroupView(AssetReturnGroupView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedBatchSize + (double)parsedGraceDays;
    }
}
