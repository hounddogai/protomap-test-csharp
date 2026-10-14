namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using System.Globalization;

/// <summary>How screens and exports show a default scenario window.</summary>
public sealed record DefaultScenarioWindowView(string Code, string LabelText, string Reviewer, string PinnedText, string ScoreText, string StatusLabel)
{
    public static DefaultScenarioWindowView ToDefaultScenarioWindowView(DefaultScenarioWindow item)
    {
        return new DefaultScenarioWindowView(
            item.Code,
            item.Label,
            item.Reviewer,
            $"{item.Pinned}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultScenarioWindowView> ToDefaultScenarioWindowViews(List<DefaultScenarioWindow> items)
    {
        var views = new List<DefaultScenarioWindowView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultScenarioWindowView(item));
        }
        return views;
    }

    public static double SumDefaultScenarioWindowView(DefaultScenarioWindowView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
