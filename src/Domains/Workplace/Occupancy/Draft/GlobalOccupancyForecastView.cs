namespace Acme.Workforce.Workplace.Occupancy.Draft;

using System.Globalization;

/// <summary>How screens and exports show a global occupancy forecast.</summary>
public sealed record GlobalOccupancyForecastView(string CodeText, string LabelText, string ActiveText, string CeilingText, string ExternalRefText, string Source, string RegionText, string UpdatedAt, string StatusLabel)
{
    public static GlobalOccupancyForecastView ToGlobalOccupancyForecastView(GlobalOccupancyForecast item)
    {
        return new GlobalOccupancyForecastView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Source,
            item.Region,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalOccupancyForecastView> ToGlobalOccupancyForecastViews(List<GlobalOccupancyForecast> items)
    {
        var views = new List<GlobalOccupancyForecastView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalOccupancyForecastView(item));
        }
        return views;
    }

    public static double SumGlobalOccupancyForecastView(GlobalOccupancyForecastView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedUpdatedAt;
    }
}
