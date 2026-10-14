namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a tiered asset return ledger.</summary>
public sealed record TieredAssetReturnLedgerView(string CodeText, string TitleText, string Pinned, string DurationMinutes, string SegmentText, string NameText, string JobCode, string UpdatedAt, string RateText, string StatusLabel)
{
    public static TieredAssetReturnLedgerView ToTieredAssetReturnLedgerView(TieredAssetReturnLedger item)
    {
        return new TieredAssetReturnLedgerView(
            item.Code,
            item.Title,
            $"{item.Pinned}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Name,
            item.JobCode,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredAssetReturnLedgerView> ToTieredAssetReturnLedgerViews(List<TieredAssetReturnLedger> items)
    {
        var views = new List<TieredAssetReturnLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToTieredAssetReturnLedgerView(item));
        }
        return views;
    }

    public static double SumTieredAssetReturnLedgerView(TieredAssetReturnLedgerView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedUpdatedAt + parsedRate;
    }
}
