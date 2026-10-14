namespace Acme.Workforce.Orgdesign.Scenario.Group;

using System.Globalization;

/// <summary>How screens and exports show a scenario slot.</summary>
public sealed record ScenarioSlotView(string Code, string NameText, string Unit, string AttemptsText, string Title, string SalaryBand, string TenureMonthsText, string Minimum, string StatusLabel)
{
    public static ScenarioSlotView ToScenarioSlotView(ScenarioSlot item)
    {
        return new ScenarioSlotView(
            item.Code,
            item.Name,
            item.Unit,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.SalaryBand,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioSlotView> ToScenarioSlotViews(List<ScenarioSlot> items)
    {
        var views = new List<ScenarioSlotView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioSlotView(item));
        }
        return views;
    }

    public static double SumScenarioSlotView(ScenarioSlotView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedTenureMonths + parsedMinimum;
    }
}
