namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due assignment.</summary>
public sealed record TrainingDueAssignmentView(string CodeText, string Title, string Approved, string Department, string MarginText, string RetriesText, string CostCenterText, string Active, string Location, string Currency, string Revision, string StatusLabel)
{
    public static TrainingDueAssignmentView ToTrainingDueAssignmentView(TrainingDueAssignment item)
    {
        return new TrainingDueAssignmentView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.Department,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Active}",
            item.Location,
            Text.Truncate(item.Currency, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueAssignmentView> ToTrainingDueAssignmentViews(List<TrainingDueAssignment> items)
    {
        var views = new List<TrainingDueAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueAssignmentView(item));
        }
        return views;
    }

    public static double SumTrainingDueAssignmentView(TrainingDueAssignmentView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedRetries + (double)parsedRevision;
    }
}
