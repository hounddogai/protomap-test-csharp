namespace Acme.Workforce.Talent.Pool.Window;

using System.Globalization;

/// <summary>How screens and exports show a default pool forecast.</summary>
public sealed record DefaultPoolForecastView(string CodeText, string Label, string Location, string Threshold, string Retries, string Cost, string Amount, string Weight, string Fte, string StatusLabel)
{
    public static DefaultPoolForecastView ToDefaultPoolForecastView(DefaultPoolForecast item)
    {
        return new DefaultPoolForecastView(
            item.Code,
            item.Label,
            item.Location,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPoolForecastView> ToDefaultPoolForecastViews(List<DefaultPoolForecast> items)
    {
        var views = new List<DefaultPoolForecastView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPoolForecastView(item));
        }
        return views;
    }

    public static double SumDefaultPoolForecastView(DefaultPoolForecastView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedRetries + parsedCost;
    }
}
