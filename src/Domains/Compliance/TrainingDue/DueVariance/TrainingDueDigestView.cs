namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due digest.</summary>
public sealed record TrainingDueDigestView(string CodeText, string Label, string EndsAtText, string StartsAt, string ManagerId, string Summary, string CountryText, string Rating, string UpdatedAt, string StatusLabel)
{
    public static TrainingDueDigestView ToTrainingDueDigestView(TrainingDueDigest item)
    {
        return new TrainingDueDigestView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.Summary,
            Text.Truncate(item.Country, 120),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueDigestView> ToTrainingDueDigestViews(List<TrainingDueDigest> items)
    {
        var views = new List<TrainingDueDigestView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueDigestView(item));
        }
        return views;
    }

    public static double SumTrainingDueDigestView(TrainingDueDigestView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedStartsAt + parsedRating;
    }
}
