namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using System.Globalization;

/// <summary>How screens and exports show a scenario threshold.</summary>
public sealed record ScenarioThresholdView(string CodeText, string Label, string TeamText, string GradeText, string StartsAt, string WindowDays, string Limit, string Fte, string RevisionText, string StatusLabel)
{
    public static ScenarioThresholdView ToScenarioThresholdView(ScenarioThreshold item)
    {
        return new ScenarioThresholdView(
            item.Code,
            item.Label,
            item.Team,
            item.Grade,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioThresholdView> ToScenarioThresholdViews(List<ScenarioThreshold> items)
    {
        var views = new List<ScenarioThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioThresholdView(item));
        }
        return views;
    }

    public static double SumScenarioThresholdView(ScenarioThresholdView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedWindowDays + (double)parsedLimit;
    }
}
