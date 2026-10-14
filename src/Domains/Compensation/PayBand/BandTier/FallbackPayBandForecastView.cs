namespace Acme.Workforce.Compensation.PayBand.BandTier;

using System.Globalization;

/// <summary>How screens and exports show a fallback pay band forecast.</summary>
public sealed record FallbackPayBandForecastView(string Code, string LabelText, string SourceText, string Discount, string Maximum, string StatusLabel)
{
    public static FallbackPayBandForecastView ToFallbackPayBandForecastView(FallbackPayBandForecast item)
    {
        return new FallbackPayBandForecastView(
            item.Code,
            item.Label,
            item.Source,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayBandForecastView> ToFallbackPayBandForecastViews(List<FallbackPayBandForecast> items)
    {
        var views = new List<FallbackPayBandForecastView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayBandForecastView(item));
        }
        return views;
    }

    public static double SumFallbackPayBandForecastView(FallbackPayBandForecastView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedMaximum;
    }
}
