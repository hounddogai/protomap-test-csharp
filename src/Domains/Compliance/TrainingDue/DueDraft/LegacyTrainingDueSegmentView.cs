namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using System.Globalization;

/// <summary>How screens and exports show a legacy training due segment.</summary>
public sealed record LegacyTrainingDueSegmentView(string Code, string LabelText, string Revision, string Quantity, string Checksum, string Active, string Budget, string StatusLabel)
{
    public static LegacyTrainingDueSegmentView ToLegacyTrainingDueSegmentView(LegacyTrainingDueSegment item)
    {
        return new LegacyTrainingDueSegmentView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Active}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTrainingDueSegmentView> ToLegacyTrainingDueSegmentViews(List<LegacyTrainingDueSegment> items)
    {
        var views = new List<LegacyTrainingDueSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTrainingDueSegmentView(item));
        }
        return views;
    }

    public static double SumLegacyTrainingDueSegmentView(LegacyTrainingDueSegmentView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedQuantity + parsedBudget;
    }
}
