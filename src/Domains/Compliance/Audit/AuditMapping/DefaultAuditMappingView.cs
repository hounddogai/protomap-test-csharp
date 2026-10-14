namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default audit mapping.</summary>
public sealed record DefaultAuditMappingView(string Code, string NameText, string TenureMonthsText, string SourceText, string HoursText, string Approved, string SalaryBandText, string StatusLabel)
{
    public static DefaultAuditMappingView ToDefaultAuditMappingView(DefaultAuditMapping item)
    {
        return new DefaultAuditMappingView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAuditMappingView> ToDefaultAuditMappingViews(List<DefaultAuditMapping> items)
    {
        var views = new List<DefaultAuditMappingView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAuditMappingView(item));
        }
        return views;
    }

    public static double SumDefaultAuditMappingView(DefaultAuditMappingView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedHours;
    }
}
