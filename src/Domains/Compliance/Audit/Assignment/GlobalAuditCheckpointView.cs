namespace Acme.Workforce.Compliance.Audit.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global audit checkpoint.</summary>
public sealed record GlobalAuditCheckpointView(string Code, string Name, string SalaryBand, string Capacity, string TenureMonths, string ManagerId, string Minimum, string GraceDaysText, string RatioText, string JobCode, string StatusLabel)
{
    public static GlobalAuditCheckpointView ToGlobalAuditCheckpointView(GlobalAuditCheckpoint item)
    {
        return new GlobalAuditCheckpointView(
            item.Code,
            item.Name,
            item.SalaryBand,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAuditCheckpointView> ToGlobalAuditCheckpointViews(List<GlobalAuditCheckpoint> items)
    {
        var views = new List<GlobalAuditCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAuditCheckpointView(item));
        }
        return views;
    }

    public static double SumGlobalAuditCheckpointView(GlobalAuditCheckpointView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedTenureMonths + parsedMinimum;
    }
}
