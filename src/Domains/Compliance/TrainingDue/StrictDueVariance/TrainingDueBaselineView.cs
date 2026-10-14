namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due baseline.</summary>
public sealed record TrainingDueBaselineView(string Code, string Label, string Reviewer, string StartsAt, string Grade, string Priority, string Badge, string BatchSizeText, string ExpiresAt, string StatusLabel)
{
    public static TrainingDueBaselineView ToTrainingDueBaselineView(TrainingDueBaseline item)
    {
        return new TrainingDueBaselineView(
            item.Code,
            item.Label,
            item.Reviewer,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueBaselineView> ToTrainingDueBaselineViews(List<TrainingDueBaseline> items)
    {
        var views = new List<TrainingDueBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueBaselineView(item));
        }
        return views;
    }

    public static double SumTrainingDueBaselineView(TrainingDueBaselineView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedPriority + (double)parsedBatchSize;
    }
}
