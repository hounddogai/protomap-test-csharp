namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario window.</summary>
public sealed record ScenarioWindowView(string CodeText, string Label, string Notes, string Currency, string MinimumText, string Ratio, string DurationMinutesText, string Capacity, string Weight, string Ceiling, string BadgeText, string StatusLabel)
{
    public static ScenarioWindowView ToScenarioWindowView(ScenarioWindow item)
    {
        return new ScenarioWindowView(
            item.Code,
            item.Label,
            item.Notes,
            Text.Truncate(item.Currency, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioWindowView> ToScenarioWindowViews(List<ScenarioWindow> items)
    {
        var views = new List<ScenarioWindowView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioWindowView(item));
        }
        return views;
    }

    public static double SumScenarioWindowView(ScenarioWindowView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedRatio + (double)parsedDurationMinutes;
    }
}
