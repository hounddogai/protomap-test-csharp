namespace Acme.Workforce.Orgdesign.Scenario.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted scenario group.</summary>
public sealed record WeightedScenarioGroupView(string Code, string Title, string Fte, string BadgeText, string Location, string Maximum, string StatusLabel)
{
    public static WeightedScenarioGroupView ToWeightedScenarioGroupView(WeightedScenarioGroup item)
    {
        return new WeightedScenarioGroupView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            Text.Truncate(item.Location, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedScenarioGroupView> ToWeightedScenarioGroupViews(List<WeightedScenarioGroup> items)
    {
        var views = new List<WeightedScenarioGroupView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedScenarioGroupView(item));
        }
        return views;
    }

    public static double SumWeightedScenarioGroupView(WeightedScenarioGroupView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedFte + parsedMaximum;
    }
}
