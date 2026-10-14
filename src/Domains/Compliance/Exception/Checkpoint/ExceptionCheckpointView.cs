namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a exception checkpoint.</summary>
public sealed record ExceptionCheckpointView(string Code, string Label, string UpdatedAt, string Ratio, string Threshold, string FloorValue, string Rate, string Fte, string ArchivedText, string BatchSize, string StartsAt, string StatusLabel)
{
    public static ExceptionCheckpointView ToExceptionCheckpointView(ExceptionCheckpoint item)
    {
        return new ExceptionCheckpointView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionCheckpointView> ToExceptionCheckpointViews(List<ExceptionCheckpoint> items)
    {
        var views = new List<ExceptionCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionCheckpointView(item));
        }
        return views;
    }

    public static double SumExceptionCheckpointView(ExceptionCheckpointView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRatio + parsedThreshold;
    }
}
