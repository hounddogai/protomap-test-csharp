namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using System.Globalization;

/// <summary>How screens and exports show a scenario snapshot.</summary>
public sealed record ScenarioSnapshotView(string CodeText, string Name, string Label, string Badge, string MarginText, string Grade, string DurationMinutes, string Skill, string Department, string Threshold, string ScoreText, string StatusLabel)
{
    public static ScenarioSnapshotView ToScenarioSnapshotView(ScenarioSnapshot item)
    {
        return new ScenarioSnapshotView(
            item.Code,
            item.Name,
            item.Label,
            item.Badge,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Department,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioSnapshotView> ToScenarioSnapshotViews(List<ScenarioSnapshot> items)
    {
        var views = new List<ScenarioSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioSnapshotView(item));
        }
        return views;
    }

    public static double SumScenarioSnapshotView(ScenarioSnapshotView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedDurationMinutes + parsedThreshold;
    }
}
