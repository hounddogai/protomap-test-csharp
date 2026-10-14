namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using System.Globalization;

/// <summary>How screens and exports show a batched training due mapping.</summary>
public sealed record BatchedTrainingDueMappingView(string Code, string Title, string Revision, string HoursText, string Maximum, string UnitText, string CostCenter, string StatusLabel)
{
    public static BatchedTrainingDueMappingView ToBatchedTrainingDueMappingView(BatchedTrainingDueMapping item)
    {
        return new BatchedTrainingDueMappingView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTrainingDueMappingView> ToBatchedTrainingDueMappingViews(List<BatchedTrainingDueMapping> items)
    {
        var views = new List<BatchedTrainingDueMappingView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTrainingDueMappingView(item));
        }
        return views;
    }

    public static double SumBatchedTrainingDueMappingView(BatchedTrainingDueMappingView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedHours + parsedMaximum;
    }
}
