namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario tier.</summary>
public sealed record ScenarioTierView(string Code, string Name, string Revision, string ArchivedText, string BudgetText, string Timezone, string WindowDays, string Unit, string EndsAt, string StatusLabel)
{
    public static ScenarioTierView ToScenarioTierView(ScenarioTier item)
    {
        return new ScenarioTierView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioTierView> ToScenarioTierViews(List<ScenarioTier> items)
    {
        var views = new List<ScenarioTierView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioTierView(item));
        }
        return views;
    }

    public static double SumScenarioTierView(ScenarioTierView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedBudget + (double)parsedWindowDays;
    }
}
