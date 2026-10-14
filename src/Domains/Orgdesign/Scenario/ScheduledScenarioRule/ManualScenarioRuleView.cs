namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual scenario rule.</summary>
public sealed record ManualScenarioRuleView(string Code, string Label, string TimezoneText, string Attempts, string Priority, string LocationText, string StatusLabel)
{
    public static ManualScenarioRuleView ToManualScenarioRuleView(ManualScenarioRule item)
    {
        return new ManualScenarioRuleView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Timezone,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualScenarioRuleView> ToManualScenarioRuleViews(List<ManualScenarioRule> items)
    {
        var views = new List<ManualScenarioRuleView>();
        foreach (var item in items)
        {
            views.Add(ToManualScenarioRuleView(item));
        }
        return views;
    }

    public static double SumManualScenarioRuleView(ManualScenarioRuleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedPriority;
    }
}
