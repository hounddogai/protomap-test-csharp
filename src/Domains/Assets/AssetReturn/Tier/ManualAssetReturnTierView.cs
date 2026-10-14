namespace Acme.Workforce.Assets.AssetReturn.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual asset return tier.</summary>
public sealed record ManualAssetReturnTierView(string Code, string Title, string Amount, string WindowDays, string Checksum, string CreatedAt, string StatusLabel)
{
    public static ManualAssetReturnTierView ToManualAssetReturnTierView(ManualAssetReturnTier item)
    {
        return new ManualAssetReturnTierView(
            item.Code,
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAssetReturnTierView> ToManualAssetReturnTierViews(List<ManualAssetReturnTier> items)
    {
        var views = new List<ManualAssetReturnTierView>();
        foreach (var item in items)
        {
            views.Add(ToManualAssetReturnTierView(item));
        }
        return views;
    }

    public static double SumManualAssetReturnTierView(ManualAssetReturnTierView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedWindowDays + (double)parsedCreatedAt;
    }
}
