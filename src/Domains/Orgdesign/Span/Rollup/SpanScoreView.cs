namespace Acme.Workforce.Orgdesign.Span.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a span score.</summary>
public sealed record SpanScoreView(string Code, string Title, string LocationText, string StartsAt, string GraceDays, string ChecksumText, string StatusLabel)
{
    public static SpanScoreView ToSpanScoreView(SpanScore item)
    {
        return new SpanScoreView(
            item.Code,
            item.Title,
            item.Location,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanScoreView> ToSpanScoreViews(List<SpanScore> items)
    {
        var views = new List<SpanScoreView>();
        foreach (var item in items)
        {
            views.Add(ToSpanScoreView(item));
        }
        return views;
    }

    public static double SumSpanScoreView(SpanScoreView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedGraceDays;
    }
}
