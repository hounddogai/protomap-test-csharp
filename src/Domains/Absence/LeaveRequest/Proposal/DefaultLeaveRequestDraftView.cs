namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default leave request draft.</summary>
public sealed record DefaultLeaveRequestDraftView(string Code, string LabelText, string Segment, string TenureMonths, string DurationMinutes, string DepartmentText, string StatusLabel)
{
    public static DefaultLeaveRequestDraftView ToDefaultLeaveRequestDraftView(DefaultLeaveRequestDraft item)
    {
        return new DefaultLeaveRequestDraftView(
            item.Code,
            item.Label,
            Text.Truncate(item.Segment, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultLeaveRequestDraftView> ToDefaultLeaveRequestDraftViews(List<DefaultLeaveRequestDraft> items)
    {
        var views = new List<DefaultLeaveRequestDraftView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultLeaveRequestDraftView(item));
        }
        return views;
    }

    public static double SumDefaultLeaveRequestDraftView(DefaultLeaveRequestDraftView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedDurationMinutes;
    }
}
