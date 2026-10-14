namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario baseline.</summary>
public sealed record ScenarioBaselineView(string Code, string Label, string Revision, string UnitText, string Version, string Title, string CostText, string ExpiresAt, string Reviewer, string StatusLabel)
{
    public static ScenarioBaselineView ToScenarioBaselineView(ScenarioBaseline item)
    {
        return new ScenarioBaselineView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioBaselineView> ToScenarioBaselineViews(List<ScenarioBaseline> items)
    {
        var views = new List<ScenarioBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioBaselineView(item));
        }
        return views;
    }

    public static double SumScenarioBaselineView(ScenarioBaselineView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedVersion + parsedCost;
    }
}
