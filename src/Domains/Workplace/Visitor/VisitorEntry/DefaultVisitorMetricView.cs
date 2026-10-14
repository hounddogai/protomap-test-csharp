namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default visitor metric.</summary>
public sealed record DefaultVisitorMetricView(string Code, string LabelText, string UpdatedAt, string TitleText, string Pinned, string ChannelText, string Retries, string Country, string Discount, string Timezone, string StatusLabel)
{
    public static DefaultVisitorMetricView ToDefaultVisitorMetricView(DefaultVisitorMetric item)
    {
        return new DefaultVisitorMetricView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Title,
            $"{item.Pinned}",
            Text.Truncate(item.Channel, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultVisitorMetricView> ToDefaultVisitorMetricViews(List<DefaultVisitorMetric> items)
    {
        var views = new List<DefaultVisitorMetricView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultVisitorMetricView(item));
        }
        return views;
    }

    public static double SumDefaultVisitorMetricView(DefaultVisitorMetricView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedRetries + parsedDiscount;
    }
}
