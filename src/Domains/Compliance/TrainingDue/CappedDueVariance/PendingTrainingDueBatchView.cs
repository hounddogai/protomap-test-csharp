namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using System.Globalization;

/// <summary>How screens and exports show a pending training due batch.</summary>
public sealed record PendingTrainingDueBatchView(string CodeText, string Label, string GraceDaysText, string Pinned, string PhoneText, string StatusLabel)
{
    public static PendingTrainingDueBatchView ToPendingTrainingDueBatchView(PendingTrainingDueBatch item)
    {
        return new PendingTrainingDueBatchView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingTrainingDueBatchView> ToPendingTrainingDueBatchViews(List<PendingTrainingDueBatch> items)
    {
        var views = new List<PendingTrainingDueBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPendingTrainingDueBatchView(item));
        }
        return views;
    }

    public static double SumPendingTrainingDueBatchView(PendingTrainingDueBatchView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays;
    }
}
