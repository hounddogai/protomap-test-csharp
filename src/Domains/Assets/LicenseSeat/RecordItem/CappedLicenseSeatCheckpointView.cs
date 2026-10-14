namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped license seat checkpoint.</summary>
public sealed record CappedLicenseSeatCheckpointView(string Code, string Title, string BatchSize, string StartsAt, string CostCenter, string StatusLabel)
{
    public static CappedLicenseSeatCheckpointView ToCappedLicenseSeatCheckpointView(CappedLicenseSeatCheckpoint item)
    {
        return new CappedLicenseSeatCheckpointView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedLicenseSeatCheckpointView> ToCappedLicenseSeatCheckpointViews(List<CappedLicenseSeatCheckpoint> items)
    {
        var views = new List<CappedLicenseSeatCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToCappedLicenseSeatCheckpointView(item));
        }
        return views;
    }

    public static double SumCappedLicenseSeatCheckpointView(CappedLicenseSeatCheckpointView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedStartsAt;
    }
}
