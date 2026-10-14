namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using System.Globalization;

/// <summary>How screens and exports show a scheduled training due variance.</summary>
public sealed record ScheduledTrainingDueVarianceView(string CodeText, string Name, string CreatedAt, string StartsAt, string Category, string ExternalRef, string EndsAtText, string Active, string Attempts, string UpdatedAtText, string PinnedText, string StatusLabel)
{
    public static ScheduledTrainingDueVarianceView ToScheduledTrainingDueVarianceView(ScheduledTrainingDueVariance item)
    {
        return new ScheduledTrainingDueVarianceView(
            item.Code,
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.ExternalRef,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledTrainingDueVarianceView> ToScheduledTrainingDueVarianceViews(List<ScheduledTrainingDueVariance> items)
    {
        var views = new List<ScheduledTrainingDueVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledTrainingDueVarianceView(item));
        }
        return views;
    }

    public static double SumScheduledTrainingDueVarianceView(ScheduledTrainingDueVarianceView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedStartsAt + (double)parsedEndsAt;
    }
}
