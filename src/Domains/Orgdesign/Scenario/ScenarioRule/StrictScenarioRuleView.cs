namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using System.Globalization;

/// <summary>How screens and exports show a strict scenario rule.</summary>
public sealed record StrictScenarioRuleView(string CodeText, string LabelText, string ReviewerText, string Notes, string Threshold, string CategoryText, string SalaryBandText, string UpdatedAt, string Location, string StatusLabel)
{
    public static StrictScenarioRuleView ToStrictScenarioRuleView(StrictScenarioRule item)
    {
        return new StrictScenarioRuleView(
            item.Code,
            item.Label,
            item.Reviewer,
            item.Notes,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.SalaryBand,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictScenarioRuleView> ToStrictScenarioRuleViews(List<StrictScenarioRule> items)
    {
        var views = new List<StrictScenarioRuleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictScenarioRuleView(item));
        }
        return views;
    }

    public static double SumStrictScenarioRuleView(StrictScenarioRuleView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedUpdatedAt;
    }
}
