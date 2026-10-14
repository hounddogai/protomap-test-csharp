namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional engagement metric.</summary>
public sealed record RegionalEngagementMetricView(string Code, string Label, string GraceDaysText, string Sequence, string Ceiling, string Limit, string ScoreText, string Country, string UpdatedAt, string Rate, string StatusLabel)
{
    public static RegionalEngagementMetricView ToRegionalEngagementMetricView(RegionalEngagementMetric item)
    {
        return new RegionalEngagementMetricView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalEngagementMetricView> ToRegionalEngagementMetricViews(List<RegionalEngagementMetric> items)
    {
        var views = new List<RegionalEngagementMetricView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalEngagementMetricView(item));
        }
        return views;
    }

    public static double SumRegionalEngagementMetricView(RegionalEngagementMetricView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedSequence + parsedCeiling;
    }
}
