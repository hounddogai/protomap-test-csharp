namespace Acme.Workforce.Absence.LeaveRequest.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request preference.</summary>
public sealed record LeaveRequestPreferenceView(string CodeText, string NameText, string ExternalRef, string ArchivedText, string Skill, string CreatedAtText, string LeaveDaysText, string StartsAt, string StatusLabel)
{
    public static LeaveRequestPreferenceView ToLeaveRequestPreferenceView(LeaveRequestPreference item)
    {
        return new LeaveRequestPreferenceView(
            item.Code,
            item.Name,
            item.ExternalRef,
            $"{item.Archived}",
            Text.Truncate(item.Skill, 40),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestPreferenceView> ToLeaveRequestPreferenceViews(List<LeaveRequestPreference> items)
    {
        var views = new List<LeaveRequestPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestPreferenceView(item));
        }
        return views;
    }

    public static double SumLeaveRequestPreferenceView(LeaveRequestPreferenceView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedLeaveDays + (double)parsedStartsAt;
    }
}
