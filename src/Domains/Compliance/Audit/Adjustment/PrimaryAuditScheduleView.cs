namespace Acme.Workforce.Compliance.Audit.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a primary audit schedule.</summary>
public sealed record PrimaryAuditScheduleView(string CodeText, string NameText, string Description, string GraceDays, string RatioText, string EmployeeId, string LimitText, string StartsAtText, string Minimum, string NotesText, string Rate, string StatusLabel)
{
    public static PrimaryAuditScheduleView ToPrimaryAuditScheduleView(PrimaryAuditSchedule item)
    {
        return new PrimaryAuditScheduleView(
            item.Code,
            item.Name,
            item.Description,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAuditScheduleView> ToPrimaryAuditScheduleViews(List<PrimaryAuditSchedule> items)
    {
        var views = new List<PrimaryAuditScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAuditScheduleView(item));
        }
        return views;
    }

    public static double SumPrimaryAuditScheduleView(PrimaryAuditScheduleView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedRatio + (double)parsedLimit;
    }
}
