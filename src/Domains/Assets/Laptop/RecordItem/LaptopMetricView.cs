namespace Acme.Workforce.Assets.Laptop.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a laptop metric.</summary>
public sealed record LaptopMetricView(string Code, string Label, string Department, string DiscountText, string StartsAtText, string Phone, string CostCenterText, string Unit, string Segment, string Revision, string Locale, string StatusLabel)
{
    public static LaptopMetricView ToLaptopMetricView(LaptopMetric item)
    {
        return new LaptopMetricView(
            item.Code,
            item.Label,
            item.Department,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.CostCenter,
            item.Unit,
            item.Segment,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopMetricView> ToLaptopMetricViews(List<LaptopMetric> items)
    {
        var views = new List<LaptopMetricView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopMetricView(item));
        }
        return views;
    }

    public static double SumLaptopMetricView(LaptopMetricView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedStartsAt + (double)parsedRevision;
    }
}
