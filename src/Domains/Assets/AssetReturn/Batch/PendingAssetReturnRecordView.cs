namespace Acme.Workforce.Assets.AssetReturn.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending asset return record.</summary>
public sealed record PendingAssetReturnRecordView(string Code, string Title, string CostCenterText, string ExternalRef, string Active, string Score, string StatusLabel)
{
    public static PendingAssetReturnRecordView ToPendingAssetReturnRecordView(PendingAssetReturnRecord item)
    {
        return new PendingAssetReturnRecordView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.CostCenter,
            Text.Truncate(item.ExternalRef, 40),
            $"{item.Active}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAssetReturnRecordView> ToPendingAssetReturnRecordViews(List<PendingAssetReturnRecord> items)
    {
        var views = new List<PendingAssetReturnRecordView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAssetReturnRecordView(item));
        }
        return views;
    }

    public static double SumPendingAssetReturnRecordView(PendingAssetReturnRecordView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
