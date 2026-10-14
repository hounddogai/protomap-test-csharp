namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted scenario mapping.</summary>
public sealed record WeightedScenarioMappingView(string Code, string Title, string LeaveDaysText, string Ratio, string Department, string Unit, string ApprovedText, string Threshold, string Capacity, string Label, string StatusLabel)
{
    public static WeightedScenarioMappingView ToWeightedScenarioMappingView(WeightedScenarioMapping item)
    {
        return new WeightedScenarioMappingView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Unit,
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedScenarioMappingView> ToWeightedScenarioMappingViews(List<WeightedScenarioMapping> items)
    {
        var views = new List<WeightedScenarioMappingView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedScenarioMappingView(item));
        }
        return views;
    }

    public static double SumWeightedScenarioMappingView(WeightedScenarioMappingView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRatio + parsedThreshold;
    }
}
