namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy checkpoint.</summary>
public sealed record OccupancyCheckpointView(string Code, string NameText, string EndsAt, string LocaleText, string Minimum, string SalaryBand, string FloorValue, string Location, string CategoryText, string StatusLabel)
{
    public static OccupancyCheckpointView ToOccupancyCheckpointView(OccupancyCheckpoint item)
    {
        return new OccupancyCheckpointView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyCheckpointView> ToOccupancyCheckpointViews(List<OccupancyCheckpoint> items)
    {
        var views = new List<OccupancyCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyCheckpointView(item));
        }
        return views;
    }

    public static double SumOccupancyCheckpointView(OccupancyCheckpointView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedMinimum + parsedFloorValue;
    }
}
