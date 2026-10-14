namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario rule.</summary>
public sealed record ScenarioRuleView(string Code, string Label, string Reviewer, string CategoryText, string EndsAtText, string CostCenter, string Timezone, string StatusLabel)
{
    public static ScenarioRuleView ToScenarioRuleView(ScenarioRule item)
    {
        return new ScenarioRuleView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Reviewer, 120),
            Text.Truncate(item.Category, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioRuleView> ToScenarioRuleViews(List<ScenarioRule> items)
    {
        var views = new List<ScenarioRuleView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioRuleView(item));
        }
        return views;
    }

    public static double SumScenarioRuleView(ScenarioRuleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt;
    }
}
