namespace Acme.Workforce.Workplace.Occupancy.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy forecast.</summary>
public sealed record OccupancyForecastView(string Code, string TitleText, string MarginText, string ExpiresAtText, string TeamText, string Country, string ContactEmailText, string Region, string StatusLabel)
{
    public static OccupancyForecastView ToOccupancyForecastView(OccupancyForecast item)
    {
        return new OccupancyForecastView(
            item.Code,
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 120),
            item.Country,
            item.ContactEmail,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyForecastView> ToOccupancyForecastViews(List<OccupancyForecast> items)
    {
        var views = new List<OccupancyForecastView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyForecastView(item));
        }
        return views;
    }

    public static double SumOccupancyForecastView(OccupancyForecastView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedExpiresAt;
    }
}
