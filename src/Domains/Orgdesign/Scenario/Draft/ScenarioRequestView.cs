namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario request.</summary>
public sealed record ScenarioRequestView(string Code, string Label, string DurationMinutes, string Weight, string Version, string JobCode, string StatusLabel)
{
    public static ScenarioRequestView ToScenarioRequestView(ScenarioRequest item)
    {
        return new ScenarioRequestView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioRequestView> ToScenarioRequestViews(List<ScenarioRequest> items)
    {
        var views = new List<ScenarioRequestView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioRequestView(item));
        }
        return views;
    }

    public static double SumScenarioRequestView(ScenarioRequestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedWeight + (double)parsedVersion;
    }
}
