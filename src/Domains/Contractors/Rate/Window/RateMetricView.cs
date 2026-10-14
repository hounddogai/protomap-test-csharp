namespace Acme.Workforce.Contractors.Rate.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate metric.</summary>
public sealed record RateMetricView(string Code, string NameText, string Channel, string Description, string SegmentText, string BadgeText, string OwnerId, string QuantityText, string StatusLabel)
{
    public static RateMetricView ToRateMetricView(RateMetric item)
    {
        return new RateMetricView(
            item.Code,
            item.Name,
            Text.Truncate(item.Channel, 120),
            item.Description,
            item.Segment,
            item.Badge,
            item.OwnerId,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateMetricView> ToRateMetricViews(List<RateMetric> items)
    {
        var views = new List<RateMetricView>();
        foreach (var item in items)
        {
            views.Add(ToRateMetricView(item));
        }
        return views;
    }

    public static double SumRateMetricView(RateMetricView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
