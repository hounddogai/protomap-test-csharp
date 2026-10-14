namespace Acme.Workforce.Engagement.TownHall.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall checkpoint.</summary>
public sealed record TownHallCheckpointView(string Code, string Title, string FloorValueText, string Margin, string Name, string Region, string LeaveDays, string Segment, string StatusLabel)
{
    public static TownHallCheckpointView ToTownHallCheckpointView(TownHallCheckpoint item)
    {
        return new TownHallCheckpointView(
            item.Code,
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Region,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallCheckpointView> ToTownHallCheckpointViews(List<TownHallCheckpoint> items)
    {
        var views = new List<TownHallCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallCheckpointView(item));
        }
        return views;
    }

    public static double SumTownHallCheckpointView(TownHallCheckpointView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMargin + (double)parsedLeaveDays;
    }
}
