namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty checkpoint.</summary>
public sealed record WarrantyCheckpointView(string Code, string Title, string BadgeText, string LimitText, string Timezone, string Name, string Weight, string WindowDays, string StatusLabel)
{
    public static WarrantyCheckpointView ToWarrantyCheckpointView(WarrantyCheckpoint item)
    {
        return new WarrantyCheckpointView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Badge,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.Name, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyCheckpointView> ToWarrantyCheckpointViews(List<WarrantyCheckpoint> items)
    {
        var views = new List<WarrantyCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyCheckpointView(item));
        }
        return views;
    }

    public static double SumWarrantyCheckpointView(WarrantyCheckpointView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedWeight + (double)parsedWindowDays;
    }
}
