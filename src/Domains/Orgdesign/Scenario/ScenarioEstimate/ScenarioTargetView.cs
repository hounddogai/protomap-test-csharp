namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario target.</summary>
public sealed record ScenarioTargetView(string CodeText, string NameText, string EmployeeId, string CreatedAtText, string WindowDays, string StatusLabel)
{
    public static ScenarioTargetView ToScenarioTargetView(ScenarioTarget item)
    {
        return new ScenarioTargetView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.EmployeeId, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioTargetView> ToScenarioTargetViews(List<ScenarioTarget> items)
    {
        var views = new List<ScenarioTargetView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioTargetView(item));
        }
        return views;
    }

    public static double SumScenarioTargetView(ScenarioTargetView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedWindowDays;
    }
}
