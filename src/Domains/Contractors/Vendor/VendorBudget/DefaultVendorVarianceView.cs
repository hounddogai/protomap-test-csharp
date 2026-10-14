namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default vendor variance.</summary>
public sealed record DefaultVendorVarianceView(string Code, string TitleText, string Rate, string Visible, string Category, string Phone, string Pinned, string Department, string WindowDaysText, string GradeText, string StatusLabel)
{
    public static DefaultVendorVarianceView ToDefaultVendorVarianceView(DefaultVendorVariance item)
    {
        return new DefaultVendorVarianceView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Category,
            Text.Truncate(item.Phone, 40),
            $"{item.Pinned}",
            item.Department,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultVendorVarianceView> ToDefaultVendorVarianceViews(List<DefaultVendorVariance> items)
    {
        var views = new List<DefaultVendorVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultVendorVarianceView(item));
        }
        return views;
    }

    public static double SumDefaultVendorVarianceView(DefaultVendorVarianceView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedWindowDays;
    }
}
