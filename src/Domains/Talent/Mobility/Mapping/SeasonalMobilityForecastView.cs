namespace Acme.Workforce.Talent.Mobility.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal mobility forecast.</summary>
public sealed record SeasonalMobilityForecastView(string CodeText, string Label, string Fte, string Summary, string Priority, string CeilingText, string ExternalRef, string ReviewerText, string OwnerIdText, string RatioText, string EmployeeIdText, string StatusLabel)
{
    public static SeasonalMobilityForecastView ToSeasonalMobilityForecastView(SeasonalMobilityForecast item)
    {
        return new SeasonalMobilityForecastView(
            item.Code,
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            Text.Truncate(item.Reviewer, 120),
            item.OwnerId,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalMobilityForecastView> ToSeasonalMobilityForecastViews(List<SeasonalMobilityForecast> items)
    {
        var views = new List<SeasonalMobilityForecastView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalMobilityForecastView(item));
        }
        return views;
    }

    public static double SumSeasonalMobilityForecastView(SeasonalMobilityForecastView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedPriority + parsedCeiling;
    }
}
