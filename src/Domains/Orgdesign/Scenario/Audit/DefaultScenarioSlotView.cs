namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using System.Globalization;

/// <summary>How screens and exports show a default scenario slot.</summary>
public sealed record DefaultScenarioSlotView(string Code, string Title, string Archived, string Weight, string Label, string UnitText, string CeilingText, string OwnerId, string StatusLabel)
{
    public static DefaultScenarioSlotView ToDefaultScenarioSlotView(DefaultScenarioSlot item)
    {
        return new DefaultScenarioSlotView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            item.Unit,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultScenarioSlotView> ToDefaultScenarioSlotViews(List<DefaultScenarioSlot> items)
    {
        var views = new List<DefaultScenarioSlotView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultScenarioSlotView(item));
        }
        return views;
    }

    public static double SumDefaultScenarioSlotView(DefaultScenarioSlotView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedCeiling;
    }
}
