namespace Acme.Workforce.Workplace.Booking.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending booking digest.</summary>
public sealed record PendingBookingDigestView(string Code, string Label, string ThresholdText, string NameText, string VersionText, string SummaryText, string CapacityText, string Category, string GraceDays, string StatusLabel)
{
    public static PendingBookingDigestView ToPendingBookingDigestView(PendingBookingDigest item)
    {
        return new PendingBookingDigestView(
            item.Code,
            item.Label,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingBookingDigestView> ToPendingBookingDigestViews(List<PendingBookingDigest> items)
    {
        var views = new List<PendingBookingDigestView>();
        foreach (var item in items)
        {
            views.Add(ToPendingBookingDigestView(item));
        }
        return views;
    }

    public static double SumPendingBookingDigestView(PendingBookingDigestView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedVersion + (double)parsedCapacity;
    }
}
