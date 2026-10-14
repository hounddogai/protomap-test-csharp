namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive scenario template.</summary>
public sealed record AdaptiveScenarioTemplateView(string Code, string Name, string LeaveDaysText, string BatchSize, string EndsAt, string Sequence, string CreatedAt, string Title, string StartsAt, string Summary, string StatusLabel)
{
    public static AdaptiveScenarioTemplateView ToAdaptiveScenarioTemplateView(AdaptiveScenarioTemplate item)
    {
        return new AdaptiveScenarioTemplateView(
            item.Code,
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Title, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveScenarioTemplateView> ToAdaptiveScenarioTemplateViews(List<AdaptiveScenarioTemplate> items)
    {
        var views = new List<AdaptiveScenarioTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveScenarioTemplateView(item));
        }
        return views;
    }

    public static double SumAdaptiveScenarioTemplateView(AdaptiveScenarioTemplateView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedBatchSize + (double)parsedEndsAt;
    }
}
