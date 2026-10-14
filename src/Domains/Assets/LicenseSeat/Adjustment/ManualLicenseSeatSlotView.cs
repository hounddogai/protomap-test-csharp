namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual license seat slot.</summary>
public sealed record ManualLicenseSeatSlotView(string CodeText, string Name, string Hours, string DurationMinutesText, string Region, string StatusLabel)
{
    public static ManualLicenseSeatSlotView ToManualLicenseSeatSlotView(ManualLicenseSeatSlot item)
    {
        return new ManualLicenseSeatSlotView(
            item.Code,
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualLicenseSeatSlotView> ToManualLicenseSeatSlotViews(List<ManualLicenseSeatSlot> items)
    {
        var views = new List<ManualLicenseSeatSlotView>();
        foreach (var item in items)
        {
            views.Add(ToManualLicenseSeatSlotView(item));
        }
        return views;
    }

    public static double SumManualLicenseSeatSlotView(ManualLicenseSeatSlotView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedDurationMinutes;
    }
}
