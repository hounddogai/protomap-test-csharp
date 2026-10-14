namespace Acme.Workforce.Payrollops.PayRun.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run metric.</summary>
public sealed record PayRunMetricView(string CodeText, string TitleText, string Threshold, string Timezone, string Channel, string StatusLabel)
{
    public static PayRunMetricView ToPayRunMetricView(PayRunMetric item)
    {
        return new PayRunMetricView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.Channel, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunMetricView> ToPayRunMetricViews(List<PayRunMetric> items)
    {
        var views = new List<PayRunMetricView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunMetricView(item));
        }
        return views;
    }

    public static double SumPayRunMetricView(PayRunMetricView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
