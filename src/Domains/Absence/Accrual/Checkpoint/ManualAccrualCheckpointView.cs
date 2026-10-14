namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual accrual checkpoint.</summary>
public sealed record ManualAccrualCheckpointView(string Code, string Title, string Sequence, string SummaryText, string Label, string Weight, string Category, string Rating, string StatusLabel)
{
    public static ManualAccrualCheckpointView ToManualAccrualCheckpointView(ManualAccrualCheckpoint item)
    {
        return new ManualAccrualCheckpointView(
            item.Code,
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAccrualCheckpointView> ToManualAccrualCheckpointViews(List<ManualAccrualCheckpoint> items)
    {
        var views = new List<ManualAccrualCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToManualAccrualCheckpointView(item));
        }
        return views;
    }

    public static double SumManualAccrualCheckpointView(ManualAccrualCheckpointView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedWeight + parsedRating;
    }
}
