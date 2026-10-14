namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work assignment.</summary>
public sealed record StatementOfWorkAssignmentView(string Code, string NameText, string CostCenterText, string SequenceText, string FloorValue, string MaximumText, string StatusLabel)
{
    public static StatementOfWorkAssignmentView ToStatementOfWorkAssignmentView(StatementOfWorkAssignment item)
    {
        return new StatementOfWorkAssignmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.CostCenter, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkAssignmentView> ToStatementOfWorkAssignmentViews(List<StatementOfWorkAssignment> items)
    {
        var views = new List<StatementOfWorkAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkAssignmentView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkAssignmentView(StatementOfWorkAssignmentView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedFloorValue + parsedMaximum;
    }
}
