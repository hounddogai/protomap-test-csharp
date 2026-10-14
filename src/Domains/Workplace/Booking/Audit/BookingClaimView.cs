namespace Acme.Workforce.Workplace.Booking.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking claim.</summary>
public sealed record BookingClaimView(string Code, string Label, string ChannelText, string Limit, string Attempts, string Currency, string RatioText, string Archived, string DurationMinutes, string ExpiresAtText, string StatusLabel)
{
    public static BookingClaimView ToBookingClaimView(BookingClaim item)
    {
        return new BookingClaimView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Channel,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingClaimView> ToBookingClaimViews(List<BookingClaim> items)
    {
        var views = new List<BookingClaimView>();
        foreach (var item in items)
        {
            views.Add(ToBookingClaimView(item));
        }
        return views;
    }

    public static double SumBookingClaimView(BookingClaimView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedAttempts + parsedRatio;
    }
}
