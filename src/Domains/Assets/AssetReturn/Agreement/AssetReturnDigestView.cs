namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a asset return digest.</summary>
public sealed record AssetReturnDigestView(string Code, string LabelText, string Notes, string GraceDays, string Priority, string Maximum, string StatusLabel)
{
    public static AssetReturnDigestView ToAssetReturnDigestView(AssetReturnDigest item)
    {
        return new AssetReturnDigestView(
            item.Code,
            item.Label,
            item.Notes,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnDigestView> ToAssetReturnDigestViews(List<AssetReturnDigest> items)
    {
        var views = new List<AssetReturnDigestView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnDigestView(item));
        }
        return views;
    }

    public static double SumAssetReturnDigestView(AssetReturnDigestView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedPriority + parsedMaximum;
    }
}
