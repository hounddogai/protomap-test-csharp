namespace Acme.Workforce.Orgdesign.Scenario.Notice;

using System.Globalization;

/// <summary>How screens and exports show a pending scenario checkpoint.</summary>
public sealed record PendingScenarioCheckpointView(string CodeText, string Label, string Capacity, string EndsAt, string FloorValueText, string Country, string TeamText, string FteText, string Threshold, string StatusLabel)
{
    public static PendingScenarioCheckpointView ToPendingScenarioCheckpointView(PendingScenarioCheckpoint item)
    {
        return new PendingScenarioCheckpointView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Team,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingScenarioCheckpointView> ToPendingScenarioCheckpointViews(List<PendingScenarioCheckpoint> items)
    {
        var views = new List<PendingScenarioCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToPendingScenarioCheckpointView(item));
        }
        return views;
    }

    public static double SumPendingScenarioCheckpointView(PendingScenarioCheckpointView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedEndsAt + parsedFloorValue;
    }
}
