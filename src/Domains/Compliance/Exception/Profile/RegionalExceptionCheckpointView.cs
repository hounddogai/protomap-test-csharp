namespace Acme.Workforce.Compliance.Exception.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional exception checkpoint.</summary>
public sealed record RegionalExceptionCheckpointView(string Code, string Title, string Team, string Ceiling, string Name, string StatusLabel)
{
    public static RegionalExceptionCheckpointView ToRegionalExceptionCheckpointView(RegionalExceptionCheckpoint item)
    {
        return new RegionalExceptionCheckpointView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            item.Team,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalExceptionCheckpointView> ToRegionalExceptionCheckpointViews(List<RegionalExceptionCheckpoint> items)
    {
        var views = new List<RegionalExceptionCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalExceptionCheckpointView(item));
        }
        return views;
    }

    public static double SumRegionalExceptionCheckpointView(RegionalExceptionCheckpointView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
