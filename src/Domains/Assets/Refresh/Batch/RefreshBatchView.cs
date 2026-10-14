namespace Acme.Workforce.Assets.Refresh.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh batch.</summary>
public sealed record RefreshBatchView(string CodeText, string Label, string Currency, string Channel, string Grade, string FloorValueText, string RevisionText, string MinimumText, string Margin, string StatusLabel)
{
    public static RefreshBatchView ToRefreshBatchView(RefreshBatch item)
    {
        return new RefreshBatchView(
            item.Code,
            item.Label,
            Text.Truncate(item.Currency, 80),
            item.Channel,
            item.Grade,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshBatchView> ToRefreshBatchViews(List<RefreshBatch> items)
    {
        var views = new List<RefreshBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshBatchView(item));
        }
        return views;
    }

    public static double SumRefreshBatchView(RefreshBatchView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRevision + parsedMinimum;
    }
}
