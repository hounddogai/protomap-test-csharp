namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual scenario estimate.</summary>
public sealed record ManualScenarioEstimateView(string Code, string Name, string SalaryBandText, string Weight, string Location, string Archived, string StatusLabel)
{
    public static ManualScenarioEstimateView ToManualScenarioEstimateView(ManualScenarioEstimate item)
    {
        return new ManualScenarioEstimateView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.SalaryBand,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 40),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualScenarioEstimateView> ToManualScenarioEstimateViews(List<ManualScenarioEstimate> items)
    {
        var views = new List<ManualScenarioEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToManualScenarioEstimateView(item));
        }
        return views;
    }

    public static double SumManualScenarioEstimateView(ManualScenarioEstimateView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
