namespace Acme.Workforce.Workplace.Booking.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default booking claim.</summary>
public sealed record DefaultBookingClaimView(string Code, string Name, string ExpiresAtText, string RegionText, string Currency, string ChannelText, string Location, string WindowDays, string SourceText, string BatchSize, string StartsAtText, string StatusLabel)
{
    public static DefaultBookingClaimView ToDefaultBookingClaimView(DefaultBookingClaim item)
    {
        return new DefaultBookingClaimView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Currency,
            Text.Truncate(item.Channel, 120),
            item.Location,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultBookingClaimView> ToDefaultBookingClaimViews(List<DefaultBookingClaim> items)
    {
        var views = new List<DefaultBookingClaimView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultBookingClaimView(item));
        }
        return views;
    }

    public static double SumDefaultBookingClaimView(DefaultBookingClaimView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedWindowDays + (double)parsedBatchSize;
    }
}
