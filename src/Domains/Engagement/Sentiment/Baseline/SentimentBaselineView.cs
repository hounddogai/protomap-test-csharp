namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment baseline.</summary>
public sealed record SentimentBaselineView(string Code, string LabelText, string Version, string WindowDays, string Department, string CategoryText, string SalaryBandText, string Timezone, string Name, string StatusLabel)
{
    public static SentimentBaselineView ToSentimentBaselineView(SentimentBaseline item)
    {
        return new SentimentBaselineView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 64),
            Text.Truncate(item.Category, 120),
            item.SalaryBand,
            item.Timezone,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentBaselineView> ToSentimentBaselineViews(List<SentimentBaseline> items)
    {
        var views = new List<SentimentBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentBaselineView(item));
        }
        return views;
    }

    public static double SumSentimentBaselineView(SentimentBaselineView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedWindowDays;
    }
}
