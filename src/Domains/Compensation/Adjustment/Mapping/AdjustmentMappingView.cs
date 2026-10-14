namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment mapping.</summary>
public sealed record AdjustmentMappingView(string Code, string Title, string MinimumText, string Department, string TenureMonths, string Ratio, string Region, string EmployeeId, string HoursText, string Grade, string ContactEmailText, string StatusLabel)
{
    public static AdjustmentMappingView ToAdjustmentMappingView(AdjustmentMapping item)
    {
        return new AdjustmentMappingView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            item.EmployeeId,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentMappingView> ToAdjustmentMappingViews(List<AdjustmentMapping> items)
    {
        var views = new List<AdjustmentMappingView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentMappingView(item));
        }
        return views;
    }

    public static double SumAdjustmentMappingView(AdjustmentMappingView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedTenureMonths + parsedRatio;
    }
}
