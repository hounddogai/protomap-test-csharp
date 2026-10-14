namespace Acme.Workforce.Orgdesign.Scenario.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback scenario override.</summary>
public sealed record FallbackScenarioOverrideView(string CodeText, string NameText, string Revision, string Location, string ReviewerText, string Archived, string Threshold, string BatchSize, string WeightText, string RegionText, string StatusLabel)
{
    public static FallbackScenarioOverrideView ToFallbackScenarioOverrideView(FallbackScenarioOverride item)
    {
        return new FallbackScenarioOverrideView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Reviewer, 40),
            $"{item.Archived}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackScenarioOverrideView> ToFallbackScenarioOverrideViews(List<FallbackScenarioOverride> items)
    {
        var views = new List<FallbackScenarioOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackScenarioOverrideView(item));
        }
        return views;
    }

    public static double SumFallbackScenarioOverrideView(FallbackScenarioOverrideView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedThreshold + (double)parsedBatchSize;
    }
}
