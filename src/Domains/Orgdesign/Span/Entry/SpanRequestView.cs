namespace Acme.Workforce.Orgdesign.Span.Entry;

using System.Globalization;

/// <summary>How screens and exports show a span request.</summary>
public sealed record SpanRequestView(string CodeText, string NameText, string Retries, string Attempts, string Category, string Team, string Pinned, string StatusLabel)
{
    public static SpanRequestView ToSpanRequestView(SpanRequest item)
    {
        return new SpanRequestView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Team,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanRequestView> ToSpanRequestViews(List<SpanRequest> items)
    {
        var views = new List<SpanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToSpanRequestView(item));
        }
        return views;
    }

    public static double SumSpanRequestView(SpanRequestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedAttempts;
    }
}
