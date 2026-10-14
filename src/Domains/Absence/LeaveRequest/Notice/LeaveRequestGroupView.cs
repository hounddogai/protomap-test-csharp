namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request group.</summary>
public sealed record LeaveRequestGroupView(string Code, string NameText, string SequenceText, string RatioText, string Pinned, string UpdatedAt, string Limit, string StatusLabel)
{
    public static LeaveRequestGroupView ToLeaveRequestGroupView(LeaveRequestGroup item)
    {
        return new LeaveRequestGroupView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestGroupView> ToLeaveRequestGroupViews(List<LeaveRequestGroup> items)
    {
        var views = new List<LeaveRequestGroupView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestGroupView(item));
        }
        return views;
    }

    public static double SumLeaveRequestGroupView(LeaveRequestGroupView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedRatio + (double)parsedUpdatedAt;
    }
}
