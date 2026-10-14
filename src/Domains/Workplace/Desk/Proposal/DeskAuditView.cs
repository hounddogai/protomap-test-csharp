namespace Acme.Workforce.Workplace.Desk.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk audit.</summary>
public sealed record DeskAuditView(string Code, string Label, string Minimum, string WindowDays, string OwnerIdText, string Unit, string DurationMinutes, string ExternalRef, string Ceiling, string Timezone, string ManagerIdText, string StatusLabel)
{
    public static DeskAuditView ToDeskAuditView(DeskAudit item)
    {
        return new DeskAuditView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Unit,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskAuditView> ToDeskAuditViews(List<DeskAudit> items)
    {
        var views = new List<DeskAuditView>();
        foreach (var item in items)
        {
            views.Add(ToDeskAuditView(item));
        }
        return views;
    }

    public static double SumDeskAuditView(DeskAuditView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedWindowDays + (double)parsedDurationMinutes;
    }
}
