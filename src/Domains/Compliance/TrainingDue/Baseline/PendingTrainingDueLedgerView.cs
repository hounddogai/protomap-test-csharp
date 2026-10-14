namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a pending training due ledger.</summary>
public sealed record PendingTrainingDueLedgerView(string CodeText, string Name, string Retries, string Rating, string Segment, string Description, string StatusLabel)
{
    public static PendingTrainingDueLedgerView ToPendingTrainingDueLedgerView(PendingTrainingDueLedger item)
    {
        return new PendingTrainingDueLedgerView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingTrainingDueLedgerView> ToPendingTrainingDueLedgerViews(List<PendingTrainingDueLedger> items)
    {
        var views = new List<PendingTrainingDueLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToPendingTrainingDueLedgerView(item));
        }
        return views;
    }

    public static double SumPendingTrainingDueLedgerView(PendingTrainingDueLedgerView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedRating;
    }
}
