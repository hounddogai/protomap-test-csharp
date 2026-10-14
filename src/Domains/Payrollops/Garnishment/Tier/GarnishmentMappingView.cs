namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment mapping.</summary>
public sealed record GarnishmentMappingView(string CodeText, string Name, string MaximumText, string Visible, string DiscountText, string Approved, string CountryText, string Version, string StatusLabel)
{
    public static GarnishmentMappingView ToGarnishmentMappingView(GarnishmentMapping item)
    {
        return new GarnishmentMappingView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Country,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentMappingView> ToGarnishmentMappingViews(List<GarnishmentMapping> items)
    {
        var views = new List<GarnishmentMappingView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentMappingView(item));
        }
        return views;
    }

    public static double SumGarnishmentMappingView(GarnishmentMappingView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedDiscount + (double)parsedVersion;
    }
}
