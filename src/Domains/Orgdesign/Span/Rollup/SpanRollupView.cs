namespace Acme.Workforce.Orgdesign.Span.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a span rollup.</summary>
public sealed record SpanRollupView(string CodeText, string NameText, string Score, string VersionText, string StatusLabel)
{
    public static SpanRollupView ToSpanRollupView(SpanRollup item)
    {
        return new SpanRollupView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanRollupView> ToSpanRollupViews(List<SpanRollup> items)
    {
        var views = new List<SpanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToSpanRollupView(item));
        }
        return views;
    }

    public static double SumSpanRollupView(SpanRollupView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedVersion;
    }
}
