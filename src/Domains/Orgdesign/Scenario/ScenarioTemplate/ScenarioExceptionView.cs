namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario exception.</summary>
public sealed record ScenarioExceptionView(string Code, string Label, string Active, string Phone, string ManagerId, string Checksum, string Archived, string CapacityText, string EndsAt, string BatchSize, string StatusLabel)
{
    public static ScenarioExceptionView ToScenarioExceptionView(ScenarioException item)
    {
        return new ScenarioExceptionView(
            item.Code,
            Text.Truncate(item.Label, 120),
            $"{item.Active}",
            Text.Truncate(item.Phone, 120),
            item.ManagerId,
            item.Checksum,
            $"{item.Archived}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioExceptionView> ToScenarioExceptionViews(List<ScenarioException> items)
    {
        var views = new List<ScenarioExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioExceptionView(item));
        }
        return views;
    }

    public static double SumScenarioExceptionView(ScenarioExceptionView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedEndsAt + (double)parsedBatchSize;
    }
}
