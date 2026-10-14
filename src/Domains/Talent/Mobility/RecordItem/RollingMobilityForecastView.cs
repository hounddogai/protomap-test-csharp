namespace Acme.Workforce.Talent.Mobility.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a rolling mobility forecast.</summary>
public sealed record RollingMobilityForecastView(string CodeText, string TitleText, string ContactEmail, string Description, string Quantity, string Location, string StatusLabel)
{
    public static RollingMobilityForecastView ToRollingMobilityForecastView(RollingMobilityForecast item)
    {
        return new RollingMobilityForecastView(
            item.Code,
            item.Title,
            item.ContactEmail,
            item.Description,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingMobilityForecastView> ToRollingMobilityForecastViews(List<RollingMobilityForecast> items)
    {
        var views = new List<RollingMobilityForecastView>();
        foreach (var item in items)
        {
            views.Add(ToRollingMobilityForecastView(item));
        }
        return views;
    }

    public static double SumRollingMobilityForecastView(RollingMobilityForecastView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
