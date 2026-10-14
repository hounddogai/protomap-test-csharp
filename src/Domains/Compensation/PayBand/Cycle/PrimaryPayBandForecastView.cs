namespace Acme.Workforce.Compensation.PayBand.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a primary pay band forecast.</summary>
public sealed record PrimaryPayBandForecastView(string Code, string Label, string GraceDaysText, string Score, string Discount, string StatusLabel)
{
    public static PrimaryPayBandForecastView ToPrimaryPayBandForecastView(PrimaryPayBandForecast item)
    {
        return new PrimaryPayBandForecastView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPayBandForecastView> ToPrimaryPayBandForecastViews(List<PrimaryPayBandForecast> items)
    {
        var views = new List<PrimaryPayBandForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPayBandForecastView(item));
        }
        return views;
    }

    public static double SumPrimaryPayBandForecastView(PrimaryPayBandForecastView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedScore + parsedDiscount;
    }
}
