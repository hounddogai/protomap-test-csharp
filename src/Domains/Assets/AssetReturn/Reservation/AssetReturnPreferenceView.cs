namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a asset return preference.</summary>
public sealed record AssetReturnPreferenceView(string Code, string Label, string Cost, string PhoneText, string Rate, string LocationText, string Retries, string Timezone, string LeaveDaysText, string StatusLabel)
{
    public static AssetReturnPreferenceView ToAssetReturnPreferenceView(AssetReturnPreference item)
    {
        return new AssetReturnPreferenceView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssetReturnPreferenceView> ToAssetReturnPreferenceViews(List<AssetReturnPreference> items)
    {
        var views = new List<AssetReturnPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToAssetReturnPreferenceView(item));
        }
        return views;
    }

    public static double SumAssetReturnPreferenceView(AssetReturnPreferenceView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedCost + parsedRate + (double)parsedRetries;
    }
}
