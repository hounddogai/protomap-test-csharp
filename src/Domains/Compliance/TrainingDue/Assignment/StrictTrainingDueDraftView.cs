namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict training due draft.</summary>
public sealed record StrictTrainingDueDraftView(string Code, string Title, string Ceiling, string UpdatedAt, string Rating, string ExternalRef, string EmployeeId, string ChannelText, string StatusLabel)
{
    public static StrictTrainingDueDraftView ToStrictTrainingDueDraftView(StrictTrainingDueDraft item)
    {
        return new StrictTrainingDueDraftView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.EmployeeId,
            Text.Truncate(item.Channel, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictTrainingDueDraftView> ToStrictTrainingDueDraftViews(List<StrictTrainingDueDraft> items)
    {
        var views = new List<StrictTrainingDueDraftView>();
        foreach (var item in items)
        {
            views.Add(ToStrictTrainingDueDraftView(item));
        }
        return views;
    }

    public static double SumStrictTrainingDueDraftView(StrictTrainingDueDraftView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedUpdatedAt + parsedRating;
    }
}
