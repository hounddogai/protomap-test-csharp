namespace Acme.Workforce.Orgdesign.Span.Draft;

using System.Globalization;

/// <summary>How screens and exports show a span claim.</summary>
public sealed record SpanClaimView(string CodeText, string NameText, string Weight, string Location, string Country, string StatusLabel)
{
    public static SpanClaimView ToSpanClaimView(SpanClaim item)
    {
        return new SpanClaimView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanClaimView> ToSpanClaimViews(List<SpanClaim> items)
    {
        var views = new List<SpanClaimView>();
        foreach (var item in items)
        {
            views.Add(ToSpanClaimView(item));
        }
        return views;
    }

    public static double SumSpanClaimView(SpanClaimView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
