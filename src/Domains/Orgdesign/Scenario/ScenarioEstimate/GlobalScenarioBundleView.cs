namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global scenario bundle.</summary>
public sealed record GlobalScenarioBundleView(string Code, string Label, string Country, string Revision, string EmployeeIdText, string StatusLabel)
{
    public static GlobalScenarioBundleView ToGlobalScenarioBundleView(GlobalScenarioBundle item)
    {
        return new GlobalScenarioBundleView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Country,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalScenarioBundleView> ToGlobalScenarioBundleViews(List<GlobalScenarioBundle> items)
    {
        var views = new List<GlobalScenarioBundleView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalScenarioBundleView(item));
        }
        return views;
    }

    public static double SumGlobalScenarioBundleView(GlobalScenarioBundleView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
