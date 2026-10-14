namespace Acme.Workforce.Engagement.TownHall.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global town hall checkpoint.</summary>
public sealed record GlobalTownHallCheckpointView(string CodeText, string Label, string HoursText, string Badge, string RetriesText, string ThresholdText, string GradeText, string TeamText, string FloorValueText, string StatusLabel)
{
    public static GlobalTownHallCheckpointView ToGlobalTownHallCheckpointView(GlobalTownHallCheckpoint item)
    {
        return new GlobalTownHallCheckpointView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Team,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalTownHallCheckpointView> ToGlobalTownHallCheckpointViews(List<GlobalTownHallCheckpoint> items)
    {
        var views = new List<GlobalTownHallCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalTownHallCheckpointView(item));
        }
        return views;
    }

    public static double SumGlobalTownHallCheckpointView(GlobalTownHallCheckpointView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedRetries + parsedThreshold;
    }
}
