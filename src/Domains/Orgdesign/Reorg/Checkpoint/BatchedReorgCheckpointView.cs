namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched reorg checkpoint.</summary>
public sealed record BatchedReorgCheckpointView(string CodeText, string Title, string SalaryBand, string DurationMinutesText, string Segment, string FloorValue, string ChannelText, string JobCode, string StatusLabel)
{
    public static BatchedReorgCheckpointView ToBatchedReorgCheckpointView(BatchedReorgCheckpoint item)
    {
        return new BatchedReorgCheckpointView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.SalaryBand,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedReorgCheckpointView> ToBatchedReorgCheckpointViews(List<BatchedReorgCheckpoint> items)
    {
        var views = new List<BatchedReorgCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedReorgCheckpointView(item));
        }
        return views;
    }

    public static double SumBatchedReorgCheckpointView(BatchedReorgCheckpointView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedFloorValue;
    }
}
