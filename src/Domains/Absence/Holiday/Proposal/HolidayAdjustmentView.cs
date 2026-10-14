namespace Acme.Workforce.Absence.Holiday.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday adjustment.</summary>
public sealed record HolidayAdjustmentView(string Code, string Label, string FloorValueText, string Badge, string Phone, string RatioText, string Version, string GraceDays, string StatusLabel)
{
    public static HolidayAdjustmentView ToHolidayAdjustmentView(HolidayAdjustment item)
    {
        return new HolidayAdjustmentView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.Phone, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayAdjustmentView> ToHolidayAdjustmentViews(List<HolidayAdjustment> items)
    {
        var views = new List<HolidayAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayAdjustmentView(item));
        }
        return views;
    }

    public static double SumHolidayAdjustmentView(HolidayAdjustmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedRatio + (double)parsedVersion;
    }
}
