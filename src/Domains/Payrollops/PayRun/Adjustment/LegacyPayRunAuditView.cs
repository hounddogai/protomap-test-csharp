namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a legacy pay run audit.</summary>
public sealed record LegacyPayRunAuditView(string CodeText, string Name, string Ceiling, string Checksum, string Skill, string Segment, string Pinned, string Source, string WindowDays, string Capacity, string Region, string StatusLabel)
{
    public static LegacyPayRunAuditView ToLegacyPayRunAuditView(LegacyPayRunAudit item)
    {
        return new LegacyPayRunAuditView(
            item.Code,
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Skill,
            item.Segment,
            $"{item.Pinned}",
            item.Source,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPayRunAuditView> ToLegacyPayRunAuditViews(List<LegacyPayRunAudit> items)
    {
        var views = new List<LegacyPayRunAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPayRunAuditView(item));
        }
        return views;
    }

    public static double SumLegacyPayRunAuditView(LegacyPayRunAuditView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedWindowDays + (double)parsedCapacity;
    }
}
