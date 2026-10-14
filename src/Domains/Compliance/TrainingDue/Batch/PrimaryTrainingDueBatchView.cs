namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary training due batch.</summary>
public sealed record PrimaryTrainingDueBatchView(string CodeText, string Name, string SalaryBandText, string Rating, string OwnerIdText, string Team, string LocationText, string StatusLabel)
{
    public static PrimaryTrainingDueBatchView ToPrimaryTrainingDueBatchView(PrimaryTrainingDueBatch item)
    {
        return new PrimaryTrainingDueBatchView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 80),
            item.SalaryBand,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Team,
            Text.Truncate(item.Location, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryTrainingDueBatchView> ToPrimaryTrainingDueBatchViews(List<PrimaryTrainingDueBatch> items)
    {
        var views = new List<PrimaryTrainingDueBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryTrainingDueBatchView(item));
        }
        return views;
    }

    public static double SumPrimaryTrainingDueBatchView(PrimaryTrainingDueBatchView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
