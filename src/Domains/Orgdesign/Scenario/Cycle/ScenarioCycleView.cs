namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a scenario cycle.</summary>
public sealed record ScenarioCycleView(string CodeText, string LabelText, string FloorValueText, string Country, string PriorityText, string LeaveDays, string Quantity, string DurationMinutes, string PhoneText, string ApprovedText, string StatusLabel)
{
    public static ScenarioCycleView ToScenarioCycleView(ScenarioCycle item)
    {
        return new ScenarioCycleView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioCycleView> ToScenarioCycleViews(List<ScenarioCycle> items)
    {
        var views = new List<ScenarioCycleView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioCycleView(item));
        }
        return views;
    }

    public static double SumScenarioCycleView(ScenarioCycleView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedPriority + (double)parsedLeaveDays;
    }
}
