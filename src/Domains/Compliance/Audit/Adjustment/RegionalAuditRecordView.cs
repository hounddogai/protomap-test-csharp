namespace Acme.Workforce.Compliance.Audit.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a regional audit record.</summary>
public sealed record RegionalAuditRecordView(string CodeText, string Label, string LeaveDaysText, string UpdatedAt, string Department, string Priority, string ManagerId, string Active, string StartsAtText, string DurationMinutes, string BadgeText, string StatusLabel)
{
    public static RegionalAuditRecordView ToRegionalAuditRecordView(RegionalAuditRecord item)
    {
        return new RegionalAuditRecordView(
            item.Code,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Active}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAuditRecordView> ToRegionalAuditRecordViews(List<RegionalAuditRecord> items)
    {
        var views = new List<RegionalAuditRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAuditRecordView(item));
        }
        return views;
    }

    public static double SumRegionalAuditRecordView(RegionalAuditRecordView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedUpdatedAt + (double)parsedPriority;
    }
}
