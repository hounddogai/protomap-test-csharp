namespace Acme.Workforce.Workplace.Booking.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback booking target.</summary>
public sealed record FallbackBookingTargetView(string CodeText, string NameText, string WindowDays, string Threshold, string Quantity, string ApprovedText, string TenureMonths, string Checksum, string StatusLabel)
{
    public static FallbackBookingTargetView ToFallbackBookingTargetView(FallbackBookingTarget item)
    {
        return new FallbackBookingTargetView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackBookingTargetView> ToFallbackBookingTargetViews(List<FallbackBookingTarget> items)
    {
        var views = new List<FallbackBookingTargetView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackBookingTargetView(item));
        }
        return views;
    }

    public static double SumFallbackBookingTargetView(FallbackBookingTargetView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedThreshold + (double)parsedQuantity;
    }
}
