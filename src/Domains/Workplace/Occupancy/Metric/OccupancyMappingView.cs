namespace Acme.Workforce.Workplace.Occupancy.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy mapping.</summary>
public sealed record OccupancyMappingView(string Code, string Name, string VersionText, string ScoreText, string Visible, string Locale, string Timezone, string EmployeeId, string CurrencyText, string Maximum, string Cost, string StatusLabel)
{
    public static OccupancyMappingView ToOccupancyMappingView(OccupancyMapping item)
    {
        return new OccupancyMappingView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Locale,
            item.Timezone,
            item.EmployeeId,
            Text.Truncate(item.Currency, 80),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyMappingView> ToOccupancyMappingViews(List<OccupancyMapping> items)
    {
        var views = new List<OccupancyMappingView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyMappingView(item));
        }
        return views;
    }

    public static double SumOccupancyMappingView(OccupancyMappingView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedScore + parsedMaximum;
    }
}
