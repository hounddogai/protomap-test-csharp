namespace Acme.Workforce.Orgdesign.Scenario.Template;

using System.Globalization;

/// <summary>How screens and exports show a scenario ledger.</summary>
public sealed record ScenarioLedgerView(string Code, string Title, string Hours, string Badge, string LeaveDays, string OwnerId, string Threshold, string BatchSize, string StartsAtText, string StatusLabel)
{
    public static ScenarioLedgerView ToScenarioLedgerView(ScenarioLedger item)
    {
        return new ScenarioLedgerView(
            item.Code,
            item.Title,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioLedgerView> ToScenarioLedgerViews(List<ScenarioLedger> items)
    {
        var views = new List<ScenarioLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioLedgerView(item));
        }
        return views;
    }

    public static double SumScenarioLedgerView(ScenarioLedgerView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedLeaveDays + parsedThreshold;
    }
}
