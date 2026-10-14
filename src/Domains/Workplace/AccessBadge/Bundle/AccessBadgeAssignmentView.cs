namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge assignment.</summary>
public sealed record AccessBadgeAssignmentView(string CodeText, string Name, string CapacityText, string Summary, string Phone, string Reviewer, string JobCode, string WindowDaysText, string StatusLabel)
{
    public static AccessBadgeAssignmentView ToAccessBadgeAssignmentView(AccessBadgeAssignment item)
    {
        return new AccessBadgeAssignmentView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Phone,
            item.Reviewer,
            item.JobCode,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeAssignmentView> ToAccessBadgeAssignmentViews(List<AccessBadgeAssignment> items)
    {
        var views = new List<AccessBadgeAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeAssignmentView(item));
        }
        return views;
    }

    public static double SumAccessBadgeAssignmentView(AccessBadgeAssignmentView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedWindowDays;
    }
}
