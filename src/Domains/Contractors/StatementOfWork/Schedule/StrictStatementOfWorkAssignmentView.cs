namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict statement of work assignment.</summary>
public sealed record StrictStatementOfWorkAssignmentView(string Code, string Name, string Margin, string SegmentText, string StartsAt, string SalaryBandText, string StatusLabel)
{
    public static StrictStatementOfWorkAssignmentView ToStrictStatementOfWorkAssignmentView(StrictStatementOfWorkAssignment item)
    {
        return new StrictStatementOfWorkAssignmentView(
            item.Code,
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictStatementOfWorkAssignmentView> ToStrictStatementOfWorkAssignmentViews(List<StrictStatementOfWorkAssignment> items)
    {
        var views = new List<StrictStatementOfWorkAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToStrictStatementOfWorkAssignmentView(item));
        }
        return views;
    }

    public static double SumStrictStatementOfWorkAssignmentView(StrictStatementOfWorkAssignmentView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedStartsAt;
    }
}
