namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a leave request milestone.</summary>
public sealed record LeaveRequestMilestoneView(string Code, string Title, string Cost, string Sequence, string Minimum, string StatusLabel)
{
    public static LeaveRequestMilestoneView ToLeaveRequestMilestoneView(LeaveRequestMilestone item)
    {
        return new LeaveRequestMilestoneView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestMilestoneView> ToLeaveRequestMilestoneViews(List<LeaveRequestMilestone> items)
    {
        var views = new List<LeaveRequestMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestMilestoneView(item));
        }
        return views;
    }

    public static double SumLeaveRequestMilestoneView(LeaveRequestMilestoneView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedSequence + parsedMinimum;
    }
}
