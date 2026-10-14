namespace Acme.Workforce.Orgdesign.Scenario.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario schedule.</summary>
public sealed record ScenarioScheduleView(string CodeText, string NameText, string Version, string BatchSize, string ExternalRef, string UnitText, string Discount, string Summary, string Priority, string RatingText, string StatusLabel)
{
    public static ScenarioScheduleView ToScenarioScheduleView(ScenarioSchedule item)
    {
        return new ScenarioScheduleView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Unit,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioScheduleView> ToScenarioScheduleViews(List<ScenarioSchedule> items)
    {
        var views = new List<ScenarioScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioScheduleView(item));
        }
        return views;
    }

    public static double SumScenarioScheduleView(ScenarioScheduleView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedBatchSize + parsedDiscount;
    }
}
