namespace Acme.Workforce.Contractors.Rate.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict rate forecast.</summary>
public sealed record StrictRateForecastView(string Code, string Title, string Maximum, string Unit, string Rating, string ExternalRef, string Threshold, string StartsAt, string Badge, string Hours, string Segment, string StatusLabel)
{
    public static StrictRateForecastView ToStrictRateForecastView(StrictRateForecast item)
    {
        return new StrictRateForecastView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictRateForecastView> ToStrictRateForecastViews(List<StrictRateForecast> items)
    {
        var views = new List<StrictRateForecastView>();
        foreach (var item in items)
        {
            views.Add(ToStrictRateForecastView(item));
        }
        return views;
    }

    public static double SumStrictRateForecastView(StrictRateForecastView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedRating + parsedThreshold;
    }
}
