namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario segment.</summary>
public sealed record ScenarioSegmentView(string Code, string Name, string Pinned, string FteText, string SummaryText, string Phone, string Quantity, string StatusLabel)
{
    public static ScenarioSegmentView ToScenarioSegmentView(ScenarioSegment item)
    {
        return new ScenarioSegmentView(
            Text.Truncate(item.Code, 120),
            item.Name,
            $"{item.Pinned}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Phone,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioSegmentView> ToScenarioSegmentViews(List<ScenarioSegment> items)
    {
        var views = new List<ScenarioSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioSegmentView(item));
        }
        return views;
    }

    public static double SumScenarioSegmentView(ScenarioSegmentView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedQuantity;
    }
}
