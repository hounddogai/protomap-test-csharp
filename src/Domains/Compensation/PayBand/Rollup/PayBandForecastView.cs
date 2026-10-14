namespace Acme.Workforce.Compensation.PayBand.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band forecast.</summary>
public sealed record PayBandForecastView(string Code, string Label, string ContactEmailText, string Department, string Rating, string Rate, string StatusLabel)
{
    public static PayBandForecastView ToPayBandForecastView(PayBandForecast item)
    {
        return new PayBandForecastView(
            item.Code,
            item.Label,
            item.ContactEmail,
            Text.Truncate(item.Department, 120),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandForecastView> ToPayBandForecastViews(List<PayBandForecast> items)
    {
        var views = new List<PayBandForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandForecastView(item));
        }
        return views;
    }

    public static double SumPayBandForecastView(PayBandForecastView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRating + parsedRate;
    }
}
