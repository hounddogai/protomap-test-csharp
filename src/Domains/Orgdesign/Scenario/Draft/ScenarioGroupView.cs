namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using System.Globalization;

/// <summary>How screens and exports show a scenario group.</summary>
public sealed record ScenarioGroupView(string Code, string TitleText, string Budget, string Margin, string PhoneText, string Label, string StatusLabel)
{
    public static ScenarioGroupView ToScenarioGroupView(ScenarioGroup item)
    {
        return new ScenarioGroupView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioGroupView> ToScenarioGroupViews(List<ScenarioGroup> items)
    {
        var views = new List<ScenarioGroupView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioGroupView(item));
        }
        return views;
    }

    public static double SumScenarioGroupView(ScenarioGroupView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMargin;
    }
}
