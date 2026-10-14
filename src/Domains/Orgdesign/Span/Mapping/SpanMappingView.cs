namespace Acme.Workforce.Orgdesign.Span.Mapping;

using System.Globalization;

/// <summary>How screens and exports show a span mapping.</summary>
public sealed record SpanMappingView(string Code, string Label, string Team, string Active, string Channel, string LeaveDays, string StatusLabel)
{
    public static SpanMappingView ToSpanMappingView(SpanMapping item)
    {
        return new SpanMappingView(
            item.Code,
            item.Label,
            item.Team,
            $"{item.Active}",
            item.Channel,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanMappingView> ToSpanMappingViews(List<SpanMapping> items)
    {
        var views = new List<SpanMappingView>();
        foreach (var item in items)
        {
            views.Add(ToSpanMappingView(item));
        }
        return views;
    }

    public static double SumSpanMappingView(SpanMappingView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
