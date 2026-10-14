namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return entry.</summary>
public sealed record AssetReturnEntryView(string Code, string Title, string Discount, string CreatedAt, string Maximum, string Summary, string Currency, string Version, string ExternalRef, string StatusLabel)
{
    public static AssetReturnEntryView ToAssetReturnEntryView(AssetReturnEntry item)
    {
        return new AssetReturnEntryView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Currency,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnEntryView> ToAssetReturnEntryViews(List<AssetReturnEntry> items)
    {
        var views = new List<AssetReturnEntryView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnEntryView(item));
        }
        return views;
    }

    public static double SumAssetReturnEntryView(AssetReturnEntryView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedCreatedAt + parsedMaximum;
    }
}
