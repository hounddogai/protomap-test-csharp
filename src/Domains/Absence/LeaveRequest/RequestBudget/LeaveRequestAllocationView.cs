namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request allocation.</summary>
public sealed record LeaveRequestAllocationView(string Code, string TitleText, string Archived, string Source, string Location, string Department, string ReviewerText, string ExpiresAtText, string Hours, string StatusLabel)
{
    public static LeaveRequestAllocationView ToLeaveRequestAllocationView(LeaveRequestAllocation item)
    {
        return new LeaveRequestAllocationView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 64),
            $"{item.Archived}",
            Text.Truncate(item.Source, 64),
            item.Location,
            item.Department,
            item.Reviewer,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestAllocationView> ToLeaveRequestAllocationViews(List<LeaveRequestAllocation> items)
    {
        var views = new List<LeaveRequestAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestAllocationView(item));
        }
        return views;
    }

    public static double SumLeaveRequestAllocationView(LeaveRequestAllocationView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedHours;
    }
}
