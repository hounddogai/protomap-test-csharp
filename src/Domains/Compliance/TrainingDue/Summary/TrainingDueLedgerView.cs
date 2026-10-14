namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due ledger.</summary>
public sealed record TrainingDueLedgerView(string Code, string Name, string ContactEmail, string RegionText, string GraceDays, string Rating, string Threshold, string Timezone, string UnitText, string Maximum, string Discount, string StatusLabel)
{
    public static TrainingDueLedgerView ToTrainingDueLedgerView(TrainingDueLedger item)
    {
        return new TrainingDueLedgerView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.ContactEmail,
            Text.Truncate(item.Region, 64),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Unit,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueLedgerView> ToTrainingDueLedgerViews(List<TrainingDueLedger> items)
    {
        var views = new List<TrainingDueLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueLedgerView(item));
        }
        return views;
    }

    public static double SumTrainingDueLedgerView(TrainingDueLedgerView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedRating + parsedThreshold;
    }
}
