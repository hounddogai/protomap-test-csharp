namespace Acme.Workforce.Payrollops.PayRun.Audit;

using System.Globalization;

/// <summary>How screens and exports show a primary pay run audit.</summary>
public sealed record PrimaryPayRunAuditView(string Code, string Title, string CountryText, string ChecksumText, string WindowDays, string Source, string VersionText, string WeightText, string DurationMinutes, string StatusLabel)
{
    public static PrimaryPayRunAuditView ToPrimaryPayRunAuditView(PrimaryPayRunAudit item)
    {
        return new PrimaryPayRunAuditView(
            item.Code,
            item.Title,
            item.Country,
            item.Checksum,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPayRunAuditView> ToPrimaryPayRunAuditViews(List<PrimaryPayRunAudit> items)
    {
        var views = new List<PrimaryPayRunAuditView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPayRunAuditView(item));
        }
        return views;
    }

    public static double SumPrimaryPayRunAuditView(PrimaryPayRunAuditView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedVersion + parsedWeight;
    }
}
