namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped scenario estimate.</summary>
public sealed record CappedScenarioEstimateView(string Code, string Title, string EmployeeIdText, string Notes, string SalaryBand, string Weight, string Ratio, string Limit, string StatusLabel)
{
    public static CappedScenarioEstimateView ToCappedScenarioEstimateView(CappedScenarioEstimate item)
    {
        return new CappedScenarioEstimateView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.EmployeeId,
            item.Notes,
            Text.Truncate(item.SalaryBand, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedScenarioEstimateView> ToCappedScenarioEstimateViews(List<CappedScenarioEstimate> items)
    {
        var views = new List<CappedScenarioEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToCappedScenarioEstimateView(item));
        }
        return views;
    }

    public static double SumCappedScenarioEstimateView(CappedScenarioEstimateView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedWeight + parsedRatio + (double)parsedLimit;
    }
}
