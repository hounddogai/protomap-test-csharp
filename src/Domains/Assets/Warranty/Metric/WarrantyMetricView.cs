namespace Acme.Workforce.Assets.Warranty.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty metric.</summary>
public sealed record WarrantyMetricView(string Code, string Title, string ReviewerText, string Region, string LeaveDays, string CreatedAt, string Badge, string Category, string Checksum, string DescriptionText, string StatusLabel)
{
    public static WarrantyMetricView ToWarrantyMetricView(WarrantyMetric item)
    {
        return new WarrantyMetricView(
            Text.Truncate(item.Code, 80),
            item.Title,
            Text.Truncate(item.Reviewer, 40),
            item.Region,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Category,
            item.Checksum,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyMetricView> ToWarrantyMetricViews(List<WarrantyMetric> items)
    {
        var views = new List<WarrantyMetricView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyMetricView(item));
        }
        return views;
    }

    public static double SumWarrantyMetricView(WarrantyMetricView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedCreatedAt;
    }
}
