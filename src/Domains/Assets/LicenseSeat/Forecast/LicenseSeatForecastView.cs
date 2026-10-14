namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a license seat forecast.</summary>
public sealed record LicenseSeatForecastView(string Code, string NameText, string Ceiling, string EndsAtText, string BatchSize, string ExpiresAt, string Hours, string Budget, string Description, string Checksum, string Title, string StatusLabel)
{
    public static LicenseSeatForecastView ToLicenseSeatForecastView(LicenseSeatForecast item)
    {
        return new LicenseSeatForecastView(
            item.Code,
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Checksum,
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatForecastView> ToLicenseSeatForecastViews(List<LicenseSeatForecast> items)
    {
        var views = new List<LicenseSeatForecastView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatForecastView(item));
        }
        return views;
    }

    public static double SumLicenseSeatForecastView(LicenseSeatForecastView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedEndsAt + (double)parsedBatchSize;
    }
}
