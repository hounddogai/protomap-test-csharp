namespace Acme.Workforce.Compliance.Audit.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled audit plan.</summary>
public sealed record ScheduledAuditPlanView(string Code, string NameText, string Revision, string Approved, string DurationMinutes, string OwnerIdText, string ExternalRef, string Grade, string Location, string LeaveDaysText, string Quantity, string StatusLabel)
{
    public static ScheduledAuditPlanView ToScheduledAuditPlanView(ScheduledAuditPlan item)
    {
        return new ScheduledAuditPlanView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.ExternalRef, 80),
            item.Grade,
            item.Location,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAuditPlanView> ToScheduledAuditPlanViews(List<ScheduledAuditPlan> items)
    {
        var views = new List<ScheduledAuditPlanView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAuditPlanView(item));
        }
        return views;
    }

    public static double SumScheduledAuditPlanView(ScheduledAuditPlanView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedDurationMinutes + (double)parsedLeaveDays;
    }
}
