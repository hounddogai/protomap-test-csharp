namespace Acme.Workforce.Assets.Laptop.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient laptop metric.</summary>
public sealed record LenientLaptopMetricView(string CodeText, string NameText, string Weight, string Region, string EndsAt, string EmployeeId, string Limit, string ApprovedText, string DescriptionText, string Badge, string Title, string StatusLabel)
{
    public static LenientLaptopMetricView ToLenientLaptopMetricView(LenientLaptopMetric item)
    {
        return new LenientLaptopMetricView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Description,
            item.Badge,
            Text.Truncate(item.Title, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLaptopMetricView> ToLenientLaptopMetricViews(List<LenientLaptopMetric> items)
    {
        var views = new List<LenientLaptopMetricView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLaptopMetricView(item));
        }
        return views;
    }

    public static double SumLenientLaptopMetricView(LenientLaptopMetricView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedEndsAt + (double)parsedLimit;
    }
}
