namespace Acme.Workforce.Orgdesign.Span.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span forecast.</summary>
public sealed record SpanForecastView(string Code, string Label, string Category, string RegionText, string EndsAtText, string Approved, string Maximum, string Location, string ExpiresAtText, string StatusLabel)
{
    public static SpanForecastView ToSpanForecastView(SpanForecast item)
    {
        return new SpanForecastView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Category, 64),
            item.Region,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanForecastView> ToSpanForecastViews(List<SpanForecast> items)
    {
        var views = new List<SpanForecastView>();
        foreach (var item in items)
        {
            views.Add(ToSpanForecastView(item));
        }
        return views;
    }

    public static double SumSpanForecastView(SpanForecastView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMaximum + (double)parsedExpiresAt;
    }
}
