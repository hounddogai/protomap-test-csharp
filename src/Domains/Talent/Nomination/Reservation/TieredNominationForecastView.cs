namespace Acme.Workforce.Talent.Nomination.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a tiered nomination forecast.</summary>
public sealed record TieredNominationForecastView(string Code, string Name, string Retries, string JobCode, string Amount, string StatusLabel)
{
    public static TieredNominationForecastView ToTieredNominationForecastView(TieredNominationForecast item)
    {
        return new TieredNominationForecastView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredNominationForecastView> ToTieredNominationForecastViews(List<TieredNominationForecast> items)
    {
        var views = new List<TieredNominationForecastView>();
        foreach (var item in items)
        {
            views.Add(ToTieredNominationForecastView(item));
        }
        return views;
    }

    public static double SumTieredNominationForecastView(TieredNominationForecastView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedAmount;
    }
}
