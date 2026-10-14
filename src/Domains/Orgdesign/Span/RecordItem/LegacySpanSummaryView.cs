namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy span summary.</summary>
public sealed record LegacySpanSummaryView(string Code, string Name, string JobCode, string RatingText, string OwnerId, string ThresholdText, string BatchSize, string EndsAt, string Limit, string DurationMinutes, string StatusLabel)
{
    public static LegacySpanSummaryView ToLegacySpanSummaryView(LegacySpanSummary item)
    {
        return new LegacySpanSummaryView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.JobCode,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacySpanSummaryView> ToLegacySpanSummaryViews(List<LegacySpanSummary> items)
    {
        var views = new List<LegacySpanSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToLegacySpanSummaryView(item));
        }
        return views;
    }

    public static double SumLegacySpanSummaryView(LegacySpanSummaryView view)
    {
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedRating + parsedThreshold + (double)parsedBatchSize;
    }
}
