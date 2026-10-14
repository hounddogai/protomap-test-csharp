namespace Acme.Workforce.Assets.Refresh.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default refresh metric.</summary>
public sealed record DefaultRefreshMetricView(string Code, string Name, string Retries, string Pinned, string ChecksumText, string Active, string Budget, string MaximumText, string Skill, string StatusLabel)
{
    public static DefaultRefreshMetricView ToDefaultRefreshMetricView(DefaultRefreshMetric item)
    {
        return new DefaultRefreshMetricView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Checksum, 64),
            $"{item.Active}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultRefreshMetricView> ToDefaultRefreshMetricViews(List<DefaultRefreshMetric> items)
    {
        var views = new List<DefaultRefreshMetricView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultRefreshMetricView(item));
        }
        return views;
    }

    public static double SumDefaultRefreshMetricView(DefaultRefreshMetricView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedBudget + parsedMaximum;
    }
}
