namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request segment.</summary>
public sealed record LeaveRequestSegmentView(string Code, string Title, string Ratio, string Skill, string Capacity, string Cost, string SalaryBandText, string TeamText, string Category, string Score, string Checksum, string StatusLabel)
{
    public static LeaveRequestSegmentView ToLeaveRequestSegmentView(LeaveRequestSegment item)
    {
        return new LeaveRequestSegmentView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            Text.Truncate(item.Team, 120),
            item.Category,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestSegmentView> ToLeaveRequestSegmentViews(List<LeaveRequestSegment> items)
    {
        var views = new List<LeaveRequestSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestSegmentView(item));
        }
        return views;
    }

    public static double SumLeaveRequestSegmentView(LeaveRequestSegmentView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedCapacity + parsedCost;
    }
}
