namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario budget.</summary>
public sealed record ScenarioBudgetView(string Code, string Label, string Hours, string Approved, string MarginText, string Badge, string Weight, string SegmentText, string UpdatedAt, string SkillText, string Ratio, string StatusLabel)
{
    public static ScenarioBudgetView ToScenarioBudgetView(ScenarioBudget item)
    {
        return new ScenarioBudgetView(
            item.Code,
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioBudgetView> ToScenarioBudgetViews(List<ScenarioBudget> items)
    {
        var views = new List<ScenarioBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioBudgetView(item));
        }
        return views;
    }

    public static double SumScenarioBudgetView(ScenarioBudgetView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedHours + parsedMargin + parsedWeight;
    }
}
