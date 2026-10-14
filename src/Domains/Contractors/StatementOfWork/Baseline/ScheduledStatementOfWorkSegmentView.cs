namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled statement of work segment.</summary>
public sealed record ScheduledStatementOfWorkSegmentView(string Code, string Name, string Country, string DurationMinutes, string Score, string EmployeeId, string StatusLabel)
{
    public static ScheduledStatementOfWorkSegmentView ToScheduledStatementOfWorkSegmentView(ScheduledStatementOfWorkSegment item)
    {
        return new ScheduledStatementOfWorkSegmentView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Country,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledStatementOfWorkSegmentView> ToScheduledStatementOfWorkSegmentViews(List<ScheduledStatementOfWorkSegment> items)
    {
        var views = new List<ScheduledStatementOfWorkSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledStatementOfWorkSegmentView(item));
        }
        return views;
    }

    public static double SumScheduledStatementOfWorkSegmentView(ScheduledStatementOfWorkSegmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedScore;
    }
}
