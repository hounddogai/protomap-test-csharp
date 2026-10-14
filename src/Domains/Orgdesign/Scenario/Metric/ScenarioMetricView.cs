namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario metric.</summary>
public sealed record ScenarioMetricView(string CodeText, string Title, string PinnedText, string Notes, string Category, string Score, string ExpiresAt, string ApprovedText, string StatusLabel)
{
    public static ScenarioMetricView ToScenarioMetricView(ScenarioMetric item)
    {
        return new ScenarioMetricView(
            item.Code,
            Text.Truncate(item.Title, 120),
            $"{item.Pinned}",
            item.Notes,
            item.Category,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioMetricView> ToScenarioMetricViews(List<ScenarioMetric> items)
    {
        var views = new List<ScenarioMetricView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioMetricView(item));
        }
        return views;
    }

    public static double SumScenarioMetricView(ScenarioMetricView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedExpiresAt;
    }
}
