namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant metric.</summary>
public sealed record EquityGrantMetricView(string Code, string LabelText, string ManagerId, string ExpiresAt, string Department, string Discount, string ExternalRef, string Priority, string StatusLabel)
{
    public static EquityGrantMetricView ToEquityGrantMetricView(EquityGrantMetric item)
    {
        return new EquityGrantMetricView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.ManagerId,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantMetricView> ToEquityGrantMetricViews(List<EquityGrantMetric> items)
    {
        var views = new List<EquityGrantMetricView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantMetricView(item));
        }
        return views;
    }

    public static double SumEquityGrantMetricView(EquityGrantMetricView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedDiscount + (double)parsedPriority;
    }
}
