namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled scenario manifest.</summary>
public sealed record ScheduledScenarioManifestView(string Code, string Name, string Limit, string ExternalRef, string Margin, string DurationMinutes, string Rating, string StatusLabel)
{
    public static ScheduledScenarioManifestView ToScheduledScenarioManifestView(ScheduledScenarioManifest item)
    {
        return new ScheduledScenarioManifestView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 40),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledScenarioManifestView> ToScheduledScenarioManifestViews(List<ScheduledScenarioManifest> items)
    {
        var views = new List<ScheduledScenarioManifestView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledScenarioManifestView(item));
        }
        return views;
    }

    public static double SumScheduledScenarioManifestView(ScheduledScenarioManifestView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMargin + (double)parsedDurationMinutes;
    }
}
