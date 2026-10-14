namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span manifest.</summary>
public sealed record SpanManifestView(string Code, string TitleText, string JobCodeText, string Fte, string Rating, string Label, string VisibleText, string BatchSize, string Region, string LeaveDays, string StatusLabel)
{
    public static SpanManifestView ToSpanManifestView(SpanManifest item)
    {
        return new SpanManifestView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.JobCode,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 40),
            $"{item.Visible}",
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanManifestView> ToSpanManifestViews(List<SpanManifest> items)
    {
        var views = new List<SpanManifestView>();
        foreach (var item in items)
        {
            views.Add(ToSpanManifestView(item));
        }
        return views;
    }

    public static double SumSpanManifestView(SpanManifestView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedFte + parsedRating + (double)parsedBatchSize;
    }
}
