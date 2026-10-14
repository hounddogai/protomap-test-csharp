namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending visitor assignment.</summary>
public sealed record PendingVisitorAssignmentView(string Code, string Label, string SequenceText, string Name, string RegionText, string ManagerIdText, string StatusLabel)
{
    public static PendingVisitorAssignmentView ToPendingVisitorAssignmentView(PendingVisitorAssignment item)
    {
        return new PendingVisitorAssignmentView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Region,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingVisitorAssignmentView> ToPendingVisitorAssignmentViews(List<PendingVisitorAssignment> items)
    {
        var views = new List<PendingVisitorAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPendingVisitorAssignmentView(item));
        }
        return views;
    }

    public static double SumPendingVisitorAssignmentView(PendingVisitorAssignmentView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
